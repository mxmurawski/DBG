#region

using DBG.Infrastructure.Enums;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Dto;
using DBG.Infrastructure.Models.Helper;
using DBG.Infrastructure.Models.Request;

#endregion

namespace DBG.Infrastructure.Interfaces;

public interface IPersistenceService
{
    UserDto ConvertUserToUserDto(User? user);
    SystemEntryDto ConvertSystemEntryToSystemEntryDto(SystemEntry entry);
    Task CheckConnectionWithPersistenceServiceAsync();
    Task PurgeSystemsStatesInDbAsync(Guid oguid, Guid dguid);
    Task<Guid> UpdateEntrysNameAsync(Guid guid, SystemsNameUpdateRequest request);
    Task<Guid> RegisterUserAsync(UserAddRequest request);
    Task<User?> GetUserAsync(Guid guid);
    Task<User?> GetUserAsync(Login login);
    Task<IList<OsStaticState>?> GetOsStaticStatesFromDbAsync(Guid guid, DateTime from, DateTime to);
    Task<IList<OsDynamicState>?> GetOsDynamicStatesFromDbAsync(Guid guid, DateTime from, DateTime to);
    Task<IList<DbDynamicState>?> GetDbDynamicStatesFromDbAsync(Guid guid, DateTime from, DateTime to);
    Task<IList<DbStaticState>?> GetDbStaticStatesFromDbAsync(Guid guid, DateTime from, DateTime to);
    Task<Guid> AddSystemEntryAsync(SystemEntryAddRequest systemEntry);
    Task<Guid> DeleteSystemEntryAsync(Guid guid);
    Task<Guid> UpdateSystemEntrysOsEntryAsync(Guid guid, OsSystemEntryUpdateRequest osSystemEntry);
    Task<Guid> UpdateSystemEntrysDbEntryAsync(Guid guid, DbSystemEntryUpdateRequest dbSystemEntry);
    Task<Guid> AddSystemEntryToDb(SystemEntry systemEntry);
    Task<SystemEntry?> GetSystemEntryFromDbAsync(Guid guid);
    Task<IList<SystemEntry>?> GetSystemEntriesFromDbAsync();
    Task<SystemEntryDto?> GetSystemEntryDtoFromDbAsync(Guid guid);
    Task<IList<SystemEntryDto>?> GetSystemEntryDtosFromDbAsync();
    Task<DbSystemEntry?> GetDbSystemFromDbAsync(Guid guid);
    Task<OsSystemEntry?> GetOsSystemFromDbAsync(Guid guid);
    Task<SystemEntry> DeleteSystemEntryFromDbAsync(SystemEntry system);
    Task<Guid> UpdateSystemEntrysDbEntryInDbAsync(Guid guid, DbSystemEntryUpdateRequest dbSystemEntry);
    Task<Guid> UpdateSystemEntrysOsEntryInDbAsync(Guid guid, OsSystemEntryUpdateRequest osSystemEntry);
    Task<Guid> GetSystemEntrysDbSystemsIdFromDbAsync(Guid guid);
    Task<Guid> GetSystemEntrysOsSystemsIdFromDbAsync(Guid guid);
    Task<bool> TestDbConnectionAsync();
    Task<bool> TestVaultConnectionAsync();
    Task<string?> SaveSecretInVaultAsync(string path, VaultEntry vaultEntry);
    Task<VaultEntry?> ReadSecretFromVaultAsync(string path);
    Task<string?> ModifySecretInVaultAsync(string path, VaultEntry vaultEntry);
    Task<string?> DeleteSecretFromVaultAsync(string path);
    Task<IList<DbSystemEntry>?> GetBareDbSystemEntriesAsync(DbType type);
    Task<IList<OsSystemEntry>?> GetBareOsSystemEntriesAsync(OsType type);
    Task<Guid> SaveOsDynamicStateAsync(OsDynamicState? osDynamicState);
    Task<Guid> SaveDbDynamicStateAsync(DbDynamicState? dbDynamicState);
    Task<Guid> SaveOsStaticStateAsync(OsStaticState? osStaticState);
    Task<Guid> SaveDbStaticStateAsync(DbStaticState? dbStaticState);
    Task<Guid> UpdateUserAsync(Guid guid, UserUpdateRequest request);
    Task<Guid> UpdateUserAsync(User user);
    Task<Guid> DeleteUserAsync(Guid guid);
    Task<UserDto?> GetUserDtoAsync(Guid guid);
    Task<IList<UserDto>?> GetUserDtosAsync();
}