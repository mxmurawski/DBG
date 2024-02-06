#region

using System.Security.Cryptography;
using System.Text;
using DBG.Infrastructure.Enums;
using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Dto;
using DBG.Infrastructure.Models.Helper;
using DBG.Infrastructure.Models.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;
using VaultSharp.V1.SecretsEngines.KeyValue.V2;

#endregion

namespace DBG.Infrastructure.Services;

public class PersistenceService : IPersistenceService
{
    private readonly DatabaseContext _databaseContext;
    private readonly ILogger<PersistenceService> _logger;
    private readonly string _mountPoint;
    private readonly IVaultClient _vaultClient;

    public PersistenceService(ILogger<PersistenceService> logger, IVaultServiceConfiguration vaultConfiguration,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        IAuthMethodInfo authMethodInfo = new TokenAuthMethodInfo(vaultConfiguration.Token);
        VaultClientSettings vaultClientSettings = new(vaultConfiguration.Address, authMethodInfo)
        {
            MyHttpClientProviderFunc = _ =>
            {
                HttpClientHandler httpClientHandler = new()
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
                return new HttpClient(httpClientHandler);
            }
        };
        _vaultClient = new VaultClient(vaultClientSettings);
        _mountPoint = vaultConfiguration.MountPoint;
        _databaseContext = scopeFactory.CreateScope().ServiceProvider.GetService<DatabaseContext>()!;
    }

    public async Task CheckConnectionWithPersistenceServiceAsync()
    {
        if (await TestDbConnectionAsync() == false || await TestVaultConnectionAsync() == false)
            throw new Exception("Cannot connect to database or vault");
    }

    public async Task<Guid> UpdateEntrysNameAsync(Guid guid, SystemsNameUpdateRequest request)
    {
        var g = await GetSystemEntryFromDbAsync(guid);
        if (g is null) throw new Exception($"System with id {guid} not found.");
        g.Name = request.Name;
        g.UpdatedOn = DateTime.Now;
        _databaseContext.SystemEntries.Update(g);
        await _databaseContext.SaveChangesAsync();
        return g.Id;
    }

    public async Task<Guid> RegisterUserAsync(UserAddRequest request)
    {
        var test = await _databaseContext.Users.Where(u => u.Email == request.Email).FirstOrDefaultAsync();
        if (test is not null) throw new Exception($"User with email {request.Email} already exists.");
        if (IsPasswordComplex(request.Password) == false) throw new Exception("Password is not complex enough");
        var g = Guid.NewGuid();
        User user = new()
        {
            Id = g,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = ComputeSha512Hash(request.Password),
            Role = request.Role,
            CreatedOn = DateTime.Now
        };
        _databaseContext.Users.Add(user);
        await _databaseContext.SaveChangesAsync();
        return g;
    }

    public async Task<User?> GetUserAsync(Guid guid)
    {
        return await _databaseContext.Users.Where(u => u.Id == guid).FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserAsync(Login login)
    {
        return await _databaseContext.Users.FirstOrDefaultAsync(u =>
            u.Email == login.Email && u.Password == ComputeSha512Hash(login.Password));
    }

    public async Task<IList<OsStaticState>?> GetOsStaticStatesFromDbAsync(Guid guid, DateTime from, DateTime to)
    {
        return await _databaseContext.OsStaticStates
            .Where(s => s.OsSystemEntryId == guid && s.Timestamp >= from && s.Timestamp <= to)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<IList<OsDynamicState>?> GetOsDynamicStatesFromDbAsync(Guid guid, DateTime from, DateTime to)
    {
        return await _databaseContext.OsDynamicStates
            .Where(s => s.OsSystemEntryId == guid && s.Timestamp >= from && s.Timestamp <= to)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<IList<DbDynamicState>?> GetDbDynamicStatesFromDbAsync(Guid guid, DateTime from, DateTime to)
    {
        return await _databaseContext.DbDynamicStates
            .Where(s => s.DbSystemEntryId == guid && s.Timestamp >= from && s.Timestamp <= to)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<IList<DbStaticState>?> GetDbStaticStatesFromDbAsync(Guid guid, DateTime from, DateTime to)
    {
        return await _databaseContext.DbStaticStates
            .Where(s => s.DbSystemEntryId == guid && s.Timestamp >= from && s.Timestamp <= to)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<Guid> AddSystemEntryAsync(SystemEntryAddRequest systemEntry)
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var guid3 = Guid.NewGuid();
        SystemEntry systemToAdd = new()
        {
            Name = systemEntry.Name,
            CreatedOn = DateTime.Now,
            OsEntry = new OsSystemEntry
            {
                Id = guid2,
                Address = systemEntry.OsEntry.Address,
                OsType = systemEntry.OsEntry.OsType,
                Port = systemEntry.OsEntry.Port,
                Username = systemEntry.OsEntry.Username,
                Password = systemEntry.OsEntry.Password
            },
            DbEntry = new DbSystemEntry
            {
                Id = guid3,
                Address = systemEntry.DbEntry.Address,
                DbType = systemEntry.DbEntry.DbType,
                Port = systemEntry.DbEntry.Port,
                Username = systemEntry.DbEntry.Username,
                Password = systemEntry.DbEntry.Password
            },
            Id = guid1
        };
        _ = await SaveSecretInVaultAsync("os/" + guid2,
            new VaultEntry { Login = systemEntry.OsEntry.Username, Password = systemEntry.OsEntry.Password });
        _ = await SaveSecretInVaultAsync("db/" + guid3,
            new VaultEntry { Login = systemEntry.DbEntry.Username, Password = systemEntry.DbEntry.Password });
        await AddSystemEntryToDb(systemToAdd);
        return guid1;
    }

    public async Task<Guid> DeleteSystemEntryAsync(Guid guid)
    {
        var system = await GetSystemEntryFromDbAsync(guid);
        if (system is null) return Guid.Empty;
        var entity = await DeleteSystemEntryFromDbAsync(system);
        await DeleteSecretFromVaultAsync("os/" + entity.OsEntry.Id);
        await DeleteSecretFromVaultAsync("db/" + entity.DbEntry.Id);
        Task.Run(()=>PurgeSystemsStatesInDbAsync(system.OsEntry.Id, system.DbEntry.Id));
        return entity.Id;
    }

    public async Task PurgeSystemsStatesInDbAsync(Guid sguid, Guid dguid)
    {
        _databaseContext.OsDynamicStates.Where(s => s.OsSystemEntryId == sguid).ExecuteDelete();
        _databaseContext.OsStaticStates.Where(s => s.OsSystemEntryId == sguid).ExecuteDelete();
        _databaseContext.DbDynamicStates.Where(s => s.DbSystemEntryId == dguid).ExecuteDelete();
        _databaseContext.DbStaticStates.Where(s => s.DbSystemEntryId == dguid).ExecuteDelete();
        await _databaseContext.SaveChangesAsync();
    }
    
    public async Task<Guid> UpdateSystemEntrysOsEntryAsync(Guid guid, OsSystemEntryUpdateRequest osSystemEntry)
    {
        var entry = await GetSystemEntryFromDbAsync(guid);
        if (entry is null) throw new Exception("System with id " + guid + " not found.");
        var g = await UpdateSystemEntrysOsEntryInDbAsync(guid, osSystemEntry);
        var e = await GetSystemEntrysOsSystemsIdFromDbAsync(guid);
        var sys = await ReadSecretFromVaultAsync("os/" + e);
        _ = await ModifySecretInVaultAsync("os/" + e, new VaultEntry
        {
            Login = osSystemEntry.Username ?? sys!.Login,
            Password = osSystemEntry.Password ?? sys!.Password
        });
        return g;
    }

    public async Task<Guid> UpdateSystemEntrysDbEntryAsync(Guid guid, DbSystemEntryUpdateRequest dbSystemEntry)
    {
        var entry = await GetSystemEntryFromDbAsync(guid);
        if (entry is null) throw new Exception("System with id " + guid + " not found.");
        var g = await UpdateSystemEntrysDbEntryInDbAsync(guid, dbSystemEntry);
        var e = await GetSystemEntrysDbSystemsIdFromDbAsync(guid);
        var sys = await ReadSecretFromVaultAsync("db/" + e);
        _ = await ModifySecretInVaultAsync("db/" + e, new VaultEntry
        {
            Login = dbSystemEntry.Username ?? sys!.Login,
            Password = dbSystemEntry.Password ?? sys!.Password
        });
        return g;
    }

    public async Task<Guid> AddSystemEntryToDb(SystemEntry systemEntry)
    {
        _databaseContext.SystemEntries.Add(systemEntry);
        await _databaseContext.SaveChangesAsync();
        return systemEntry.Id;
    }

    public async Task<SystemEntry?> GetSystemEntryFromDbAsync(Guid guid)
    {
        return await _databaseContext.SystemEntries.Select(x => x).Where(x => x.Id == guid)
            .Include(x => x.OsEntry)
            .Include(x => x.DbEntry)
            .FirstOrDefaultAsync();
    }

    public async Task<IList<SystemEntry>?> GetSystemEntriesFromDbAsync()
    {
        return await _databaseContext.SystemEntries.Select(x => x)
            .Include(x => x.OsEntry)
            .Include(x => x.DbEntry)
            .ToListAsync();
    }

    public async Task<SystemEntryDto?> GetSystemEntryDtoFromDbAsync(Guid guid)
    {
        var entry = await GetSystemEntryFromDbAsync(guid);
        if (entry is null) throw new Exception($"System entry with id {guid} does not exist.");
        return ConvertSystemEntryToSystemEntryDto(entry);
    }

    public SystemEntryDto ConvertSystemEntryToSystemEntryDto(SystemEntry entry)
    {
        return new SystemEntryDto
        {
            Id = entry.Id,
            DbEntry = new DbSystemEntryDto
            {
                Address = entry.DbEntry.Address,
                DbType = entry.DbEntry.DbType,
                Id = entry.DbEntry.Id,
                Port = entry.DbEntry.Port
            },
            Name = entry.Name,
            OsEntry = new OsSystemEntryDto
            {
                Address = entry.OsEntry.Address,
                Id = entry.OsEntry.Id,
                Port = entry.OsEntry.Port,
                OsType = entry.OsEntry.OsType
            }
        };
    }

    public async Task<IList<SystemEntryDto>?> GetSystemEntryDtosFromDbAsync()
    {
        var entries = await GetSystemEntriesFromDbAsync();
        return entries?.Select(ConvertSystemEntryToSystemEntryDto).ToList();
    }

    public async Task<DbSystemEntry?> GetDbSystemFromDbAsync(Guid guid)
    {
        return await _databaseContext.SystemEntries.Select(x => x.DbEntry).Where(x => x.Id == guid)
            .FirstOrDefaultAsync();
    }

    public async Task<OsSystemEntry?> GetOsSystemFromDbAsync(Guid guid)
    {
        return await _databaseContext.SystemEntries.Select(x => x.OsEntry).Where(x => x.Id == guid)
            .FirstOrDefaultAsync();
    }

    public async Task<SystemEntry> DeleteSystemEntryFromDbAsync(SystemEntry system)
    {
        _databaseContext.SystemEntries.Where(x => x.Id == system.Id).ExecuteDelete();
        _databaseContext.OsSystemEntries.Where(os => os.Id == system.OsEntry.Id).ExecuteDelete();
        _databaseContext.DbSystemEntries.Where(db => db.Id == system.DbEntry.Id).ExecuteDelete();
        await _databaseContext.SaveChangesAsync();
        return system;
    }

    public async Task<Guid> UpdateSystemEntrysDbEntryInDbAsync(Guid guid, DbSystemEntryUpdateRequest dbSystemEntry)
    {
        var entry = await _databaseContext.SystemEntries.Select(x => x).Where(x => x.Id == guid)
            .Include(x => x.DbEntry)
            .FirstOrDefaultAsync();
        if (entry is null) return Guid.Empty;
        entry.UpdatedOn = DateTime.Now;
        entry.DbEntry.Address = dbSystemEntry.Address ?? entry.DbEntry.Address;
        entry.DbEntry.Port = dbSystemEntry.Port != 0 ? dbSystemEntry.Port : entry.DbEntry.Port;
        await _databaseContext.SaveChangesAsync();
        return entry.Id;
    }

    public async Task<Guid> UpdateSystemEntrysOsEntryInDbAsync(Guid guid, OsSystemEntryUpdateRequest osSystemEntry)
    {
        var entry = await _databaseContext.SystemEntries.Select(x => x).Where(x => x.Id == guid)
            .Include(x => x.OsEntry)
            .FirstOrDefaultAsync();
        if (entry is null) return Guid.Empty;
        entry.UpdatedOn = DateTime.Now;
        entry.OsEntry.Port = osSystemEntry.Port != 0 ? osSystemEntry.Port : entry.OsEntry.Port;
        entry.OsEntry.Address = osSystemEntry.Address ?? entry.OsEntry.Address;
        await _databaseContext.SaveChangesAsync();
        return entry.Id;
    }

    public async Task<Guid> GetSystemEntrysDbSystemsIdFromDbAsync(Guid guid)
    {
        var system = await _databaseContext.SystemEntries.Where(x => x.Id == guid)
            .Include(x => x.DbEntry)
            .FirstOrDefaultAsync();
        return system != null ? system.DbEntry.Id : Guid.Empty;
    }

    public async Task<Guid> GetSystemEntrysOsSystemsIdFromDbAsync(Guid guid)
    {
        var system = await _databaseContext.SystemEntries.Where(x => x.Id == guid)
            .Include(x => x.OsEntry)
            .FirstOrDefaultAsync();
        return system != null ? system.OsEntry.Id : Guid.Empty;
    }

    public async Task<bool> TestDbConnectionAsync()
    {
        var check = false;
        try
        {
            check = await _databaseContext.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError( $"Cannot connect to database. {ex.Message}");
        }

        return check;
    }

    public async Task<bool> TestVaultConnectionAsync()
    {
        var check = false;
        try
        {
            check = !(await _vaultClient.V1.System.GetSealStatusAsync()).Sealed &&
                    (await _vaultClient.V1.System.GetHealthStatusAsync()).HttpStatusCode == 200;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Cannot connect to vault. {ex.Message}");
        }

        return check;
    }

    public async Task<string?> SaveSecretInVaultAsync(string path, VaultEntry vaultEntry)
    {
        if (vaultEntry.Login == null || vaultEntry.Password == null) throw new Exception("Login or password is null");
        try
        {
            await _vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(path, new Dictionary<string, object>
            {
                { "login", vaultEntry.Login },
                { "password", vaultEntry.Password }
            }, mountPoint: _mountPoint);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while saving secret in vault at path {path}. {ex.Message}");
            return null;
        }

        return path;
    }

    public async Task<VaultEntry?> ReadSecretFromVaultAsync(string path)
    {
        VaultEntry response = new();
        try
        {
            Secret<SecretData> secret =
                await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path, mountPoint: _mountPoint);
            response.Login = secret.Data.Data["login"].ToString() ?? "";
            response.Password = secret.Data.Data["password"].ToString() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while reading secret in vault at path {path}. {ex.Message}");
            return null;
        }

        return response;
    }

    public async Task<string?> ModifySecretInVaultAsync(string path, VaultEntry vaultEntry)
    {
        var secret = await ReadSecretFromVaultAsync(path);
        if (secret == null) throw new Exception($"Unable to find secret in vault at path {path}");
        Dictionary<string, object> patchData = new()
        {
            { "login", vaultEntry.Login ?? secret.Login ?? "" },
            { "password", vaultEntry.Password ?? secret.Password ?? "" }
        };
        PatchSecretDataRequest patchDataRequest = new() { Data = patchData };
        try
        {
            _ = await _vaultClient.V1.Secrets.KeyValue.V2.PatchSecretAsync(mountPoint: _mountPoint, path: path,
                patchSecretDataRequest: patchDataRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while modifying secret in vault at path {path}. {ex.Message}");
            return null;
        }

        return path;
    }

    public async Task<string?> DeleteSecretFromVaultAsync(string path)
    {
        try
        {
            await _vaultClient.V1.Secrets.KeyValue.V2.DeleteMetadataAsync(mountPoint: _mountPoint, path: path);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while deleting secret at path {path}. {ex.Message}");
            return null;
        }

        return path;
    }

    public async Task<IList<DbSystemEntry>?> GetBareDbSystemEntriesAsync(DbType type)
    {
        var systems = await _databaseContext.DbSystemEntries.AsNoTracking().Where(s => s.DbType == type).ToListAsync();
        foreach (var system in systems)
        {
            var sys = await ReadSecretFromVaultAsync("db/" + system.Id);
            if (sys == null) continue;
            system.Username = sys.Login;
            system.Password = sys.Password;
        }

        return systems;
    }

    public async Task<IList<OsSystemEntry>?> GetBareOsSystemEntriesAsync(OsType type)
    {
        var systems = await _databaseContext.OsSystemEntries.AsNoTracking().Where(s => s.OsType == type).ToListAsync();
        foreach (var system in systems)
        {
            var sys = await ReadSecretFromVaultAsync("os/" + system.Id);
            if (sys == null) continue;
            system.Username = sys.Login;
            system.Password = sys.Password;
        }

        return systems;
    }

    public async Task<Guid> SaveOsDynamicStateAsync(OsDynamicState? osDynamicState)
    {
        if (osDynamicState == null) return Guid.Empty;
        _databaseContext.OsDynamicStates.Add(osDynamicState);
        await _databaseContext.SaveChangesAsync();
        return osDynamicState.Id;
    }

    public async Task<Guid> SaveDbDynamicStateAsync(DbDynamicState? dbDynamicState)
    {
        if (dbDynamicState == null) return Guid.Empty;
        _databaseContext.DbDynamicStates.Add(dbDynamicState);
        await _databaseContext.SaveChangesAsync();
        return dbDynamicState.Id;
    }

    public async Task<Guid> SaveOsStaticStateAsync(OsStaticState? osStaticState)
    {
        if (osStaticState == null) return Guid.Empty;
        _databaseContext.OsStaticStates.Add(osStaticState);
        await _databaseContext.SaveChangesAsync();
        return osStaticState.Id;
    }

    public async Task<Guid> SaveDbStaticStateAsync(DbStaticState? dbStaticState)
    {
        if (dbStaticState == null) return Guid.Empty;
        _databaseContext.DbStaticStates.Add(dbStaticState);
        await _databaseContext.SaveChangesAsync();
        return dbStaticState.Id;
    }

    public async Task<Guid> UpdateUserAsync(Guid guid, UserUpdateRequest request)
    {
        var u1 = await _databaseContext.Users.Where(u => u.Id == guid).FirstOrDefaultAsync();
        if (u1 is null) throw new Exception($"User with id {guid} not found.");
        if (u1.Email != request.Email)
        {
            var u2 = await _databaseContext.Users.Where(u => u.Email == request.Email).FirstOrDefaultAsync();
            if (u2 is not null) throw new Exception($"User with email {request.Email} already exists.");
        }

        if (request.Password is not null && !IsPasswordComplex(request.Password!))
            throw new Exception("Password is not complex enough");
        u1.Email = request.Email ?? u1.Email;
        u1.FirstName = request.FirstName ?? u1.FirstName;
        u1.LastName = request.LastName ?? u1.LastName;
        u1.Password = request.Password is null ? u1.Password : ComputeSha512Hash(request.Password);
        u1.Role = request.Role ?? u1.Role;
        u1.UpdatedOn = DateTime.Now;
        _databaseContext.Users.Update(u1);
        await _databaseContext.SaveChangesAsync();
        return u1.Id;
    }

    public async Task<Guid> UpdateUserAsync(User user)
    {
        var u = await _databaseContext.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync();
        if (u is null) throw new Exception($"User with Id {user.Id} not found");
        _databaseContext.Users.Entry(u).CurrentValues.SetValues(user);
        await _databaseContext.SaveChangesAsync();
        return user.Id;
    }

    public async Task<Guid> DeleteUserAsync(Guid guid)
    {
        var user = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Id == guid);
        if (user is null) throw new Exception($"User with id {guid} not found");
        _databaseContext.Users.Remove(user);
        await _databaseContext.SaveChangesAsync();
        return user.Id;
    }

    public async Task<UserDto?> GetUserDtoAsync(Guid guid)
    {
        var u = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Id == guid);
        return ConvertUserToUserDto(u);
    }

    public UserDto ConvertUserToUserDto(User? user)
    {
        return user is null
            ? throw new Exception("User is null")
            : new UserDto
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Id = user.Id
            };
    }

    public async Task<IList<UserDto>?> GetUserDtosAsync()
    {
        var users = await _databaseContext.Users.ToListAsync();
        return users.Select(ConvertUserToUserDto).ToList();
    }

    public static bool IsPasswordComplex(string password)
    {
        return password.Length >= 8
               && password.Count(char.IsDigit) >= 2
               && password.Count(c => !char.IsLetterOrDigit(c)) >= 2;
    }

    public static string ComputeSha512Hash(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA512.HashData(inputBytes);
        StringBuilder stringBuilder = new();
        foreach (var t in hashBytes) _ = stringBuilder.Append(t.ToString("X2"));
        return stringBuilder.ToString();
    }
}