#region

using DBG.Infrastructure.Enums;
using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Helper;
using DBG.Infrastructure.Models.Request;
using DBG.Infrastructure.Queries;
using Renci.SshNet;

#endregion

namespace DBG.SSHWorker;

public class ConnectionManager : IConnectionManager
{
    private readonly Dictionary<Guid, SshClient> _connections;
    private readonly IPersistenceService _persistenceService;
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<ConnectionManager> logger;

    public ConnectionManager(ILogger<ConnectionManager> logger, IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        _persistenceService = scopeFactory.CreateScope().ServiceProvider.GetService<IPersistenceService>()!;
        _connections = new Dictionary<Guid, SshClient>();
        _semaphore = new SemaphoreSlim(1);
    }

    public Task ExecuteNonQueryAsync(Guid connection, string query, string database)
    {
        throw new NotImplementedException();
    }

    public async Task LoadConnectionsAsync()
    {
        var connections = await _persistenceService.GetBareOsSystemEntriesAsync(OsType.Linux);
        if (connections != null)
            foreach (var connection in connections)
                await PerformLoadConnectionsAsync(connection);
    }

    public void ReadOsDynamicState(object? state)
    {
        Task.Run(PerformReadOsDynamicStateAsync);
    }

    public void ReadOsStaticState(object? state)
    {
        Task.Run(PerformReadOsStaticStateAsync);
    }

    public void ReadDbDynamicState(object? state)
    {
        throw new NotImplementedException();
    }

    public void ReadDbStaticState(object? state)
    {
        throw new NotImplementedException();
    }

    public async Task PerformReadOsDynamicStateAsync()
    {
        foreach (var conn in _connections)
            _ = await _persistenceService.SaveOsDynamicStateAsync(await PerformReadOsDynamicStateAsync(conn.Key));
    }

    public async Task PerformReadOsStaticStateAsync()
    {
        foreach (var conn in _connections)
            _ = await _persistenceService.SaveOsStaticStateAsync(await PerformReadOsStaticStateAsync(conn.Key));
    }

    public Task PerformReadDbDynamicStateAsync()
    {
        throw new NotImplementedException();
    }

    public Task PerformReadDbStaticStateAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<OsDynamicState?> PerformReadOsDynamicStateAsync(Guid guid)
    {
        try
        {
            return new OsDynamicState
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                OsSystemEntryId = guid,
                CpuUsage = await GetCpuUsageAsync(guid),
                RamUsage = await GetRamUsageAsync(guid),
                DiskUsageAsJson = await GetDiskUsageAsync(guid)
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<OsStaticState?> PerformReadOsStaticStateAsync(Guid guid)
    {
        try
        {
            return new OsStaticState
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                OsSystemEntryId = guid,
                Version = await GetVersionAsync(guid),
                CpuCount = await GetCpuCountAsync(guid),
                RamCount = await GetRamCountAsync(guid),
                Uptime = await GetUptimeAsync(guid)
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public Task<DbDynamicState?> PerformReadDbDynamicStateAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<DbStaticState?> PerformReadDbStaticStateAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateUserAsync(Guid guid, CreateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CreateLoginAsync(Guid guid, CreateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DropUserAsync(Guid guid, DropRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DropLoginAsync(Guid guid, DropRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> GrantAsync(Guid guid, GrantOrRevokeRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RevokeAsync(Guid guid, GrantOrRevokeRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetPermissionAuditResultAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Action DisconnectAll()
    {
        return () =>
        {
            foreach (var conn in _connections) _ = DeleteConnectionAsync(conn.Key);
        };
    }

    public void LoadConnections(object? state)
    {
        Task.Run(LoadConnectionsAsync);
    }

    public void CheckIfConnectionExists(Guid guid)
    {
        if (_connections.ContainsKey(guid)) return;
        throw new Exception($"Connection with id {guid} does not exist");
    }

    public void CheckIfConnectionExists(Guid guid, string database)
    {
        throw new NotImplementedException();
    }

    public void ConnectToNewDatabases(object? state)
    {
        throw new NotImplementedException();
    }

    public Task ConnectToNewDatabasesAsync()
    {
        throw new NotImplementedException();
    }

    public Task PerformLoadConnectionsAsync(DbSystemEntry connection)
    {
        throw new NotImplementedException();
    }

    public async Task PerformLoadConnectionsAsync(OsSystemEntry connection)
    {
        if (_connections.TryGetValue(connection.Id, out var connection1))
        {
            if (connection1.IsConnected) return;
            await DeleteConnectionAsync(connection.Id);
        }
        CancellationTokenSource cts = new();
        var conn = CreateSshClient(connection);
        try
        {
            await conn.ConnectAsync(cts.Token);
            logger.LogInformation($"Connected to {connection.Address}:{connection.Port}");
            _connections.Add(connection.Id, conn);
        }
        catch (Exception ex)
        {
            logger.LogError($"Unable to connect to {connection.Address}:{connection.Port}. {ex.Message}");
        }
    }

    public async Task<bool> AddOrUpdateConnectionAsync(Guid guid)
    {
        var keyExists = _connections.ContainsKey(guid);
        if (keyExists) await DeleteConnectionAsync(guid);
        var connections = await _persistenceService.GetBareOsSystemEntriesAsync(OsType.Linux);
        var connection = connections?.FirstOrDefault(s => s.Id == guid);
        if (connection == null) return false;
        await PerformLoadConnectionsAsync(connection);
        return true;
    }

    public async Task<bool> DeleteConnectionAsync(Guid guid)
    {
        var connToDelete = _connections.FirstOrDefault(c => c.Key == guid);
        if (connToDelete.Value == null) return false;
        await _semaphore.WaitAsync();
        try
        {
            connToDelete.Value.Disconnect();
            logger.LogInformation($"Disconnected from {connToDelete.Value.ConnectionInfo.Host}:{connToDelete.Value.ConnectionInfo.Port}");
        }
        catch (Exception)
        {
            logger.LogInformation($"Unable to disconnect from {connToDelete.Value.ConnectionInfo.Host}:{connToDelete.Value.ConnectionInfo.Port}");
        }
        finally
        {
            _ = _semaphore.Release();
        }
        _ = _connections.Remove(guid);
        return true;
    }

    public Task ExecuteNonQueryAsync(Guid connection, string query)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> ExecuteCommandAsync(Guid connection, string command)
    {
        CheckIfConnectionExists(connection);
        string? result;
        await _semaphore.WaitAsync();
        try
        {
            using var c = _connections[connection].CreateCommand(command);
            result = c.Execute();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
        finally
        {
            _ = _semaphore.Release();
        }
        return result;
    }

    public Task<DbResult?> ExecuteQueryAsync(Guid connection, string query, string database)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> TestConnectivityAsync(Guid guid)
    {
        var result = await ExecuteCommandAsync(guid, "echo test");
        return result == "test\n";
    }

    public Task<string?> GetDatabaseAndTablesAsync(Guid guid)
    {
        throw new NotImplementedException();
    }
    
    public static SshClient CreateSshClient(OsSystemEntry connection)
    {
        return new SshClient(
            connection.Address,
            connection.Port,
            connection.Username,
            connection.Password
        )
        {
            KeepAliveInterval = TimeSpan.FromMinutes(1)
        };
    }

    public async Task<double> GetCpuUsageAsync(Guid guid)
    {
        var r = await ExecuteCommandAsync(guid, SSH.GetCpuUsage());
        r = r?.Replace("\n", "");
        double.TryParse(r, out var n);
        return n;
    }

    public async Task<int> GetRamUsageAsync(Guid guid)
    {
        var r = await ExecuteCommandAsync(guid, SSH.GetRamUsage());
        r = r?.Replace("\n", "");
        int.TryParse(r, out var n);
        return n;
    }

    public async Task<string?> GetDiskUsageAsync(Guid guid)
    {
        var r = await ExecuteCommandAsync(guid, SSH.GetDiskUsage());
        r = r?.Replace("\n", "");
        r = new string(r?.Where(c => !char.IsWhiteSpace(c)).ToArray());
        return r;
    }

    public async Task<string?> GetVersionAsync(Guid guid)
    {
        var r = await ExecuteCommandAsync(guid, SSH.GetVersion());
        r = r?.Replace("\n", "");
        return r;
    }

    public async Task<int> GetCpuCountAsync(Guid guid)
    {
        var r = await ExecuteCommandAsync(guid, SSH.GetCpuCount());
        r = r?.Replace("\n", "");
        int.TryParse(r, out var n);
        return n;
    }

    public async Task<int> GetRamCountAsync(Guid guid)
    {
        var r = await ExecuteCommandAsync(guid, SSH.GetRamCount());
        r = r?.Replace("\n", "");
        int.TryParse(r, out var n);
        return n;
    }

    public async Task<string?> GetUptimeAsync(Guid guid)
    {
        var r = await ExecuteCommandAsync(guid, SSH.GetUptime());
        r = r?.Replace("\n", "");
        return r;
    }
}