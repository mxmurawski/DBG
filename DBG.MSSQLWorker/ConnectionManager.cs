#region

using System.Data;
using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Helper;
using DBG.Infrastructure.Models.Request;
using DBG.Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using DbType = DBG.Infrastructure.Enums.DbType;

#endregion

namespace DBG.MSSQLWorker;

public class ConnectionManager
    : IConnectionManager
{
    private readonly Dictionary<Guid, SqlConnection> _connections;
    private readonly IPersistenceService _persistenceService;
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<ConnectionManager> logger;

    public ConnectionManager(ILogger<ConnectionManager> logger, IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        _persistenceService = scopeFactory.CreateScope().ServiceProvider.GetService<IPersistenceService>()!;
        _connections = new Dictionary<Guid, SqlConnection>();
        _semaphore = new SemaphoreSlim(1);
    }

    public Task ExecuteNonQueryAsync(Guid connection, string query)
    {
        throw new NotImplementedException();
    }

    public async Task ExecuteNonQueryAsync(Guid connection, string query, string database="master")
    {
        CheckIfConnectionExists(connection);
        await _semaphore.WaitAsync();
        try
        {
            _connections[connection].ChangeDatabase(database);
            await using SqlCommand cmd = new(query, _connections[connection]);
            _ = await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw new Exception(ex.Message);
        }
        finally
        {
            _ = _semaphore.Release();
        }
    }

    public async Task LoadConnectionsAsync()
    {
        var connections = await _persistenceService.GetBareDbSystemEntriesAsync(DbType.Mssql);
        if (connections != null)
            foreach (var connection in connections)
                await PerformLoadConnectionsAsync(connection);
    }

    public async Task<string?> GetPermissionAuditResultAsync(Guid guid)
    {
        JObject response = new();
        var serverRoles = await ExecuteQueryAsync(guid, MSSQL.GetServerRoles());
        var serverRolesList = serverRoles?.Rows
            .Select(sr => (object)new { Role = sr[0].ToString(), Members = sr[1].ToString() }).ToList();
        if (serverRolesList == null) return null;
        response.Add("ServerRoles", JToken.FromObject(serverRolesList));
        var databases = await ExecuteQueryAsync(guid, MSSQL.GetDatabases());
        var databasesList = databases?.Rows.Select(row => row[0].ToString()).ToList();
        Dictionary<string, List<object>> databaseRolesList = new();
        Dictionary<string, List<object>> databasePermissionsList = new();
        if (databasesList == null) return null;
        foreach (var d in databasesList)
        {
            var x1 = await ExecuteQueryAsync(guid,
                MSSQL.GetDatabaseRoles(),
                d);
            var x2 = await ExecuteQueryAsync(guid,
                MSSQL.GetDatabasePermissionsList(),
                d);
            if (x1 == null || x2 == null) continue;
            databaseRolesList.Add(d,
                x1.Rows.Select(row => (object)new { Rolename = row[0].ToString(), Members = row[1].ToString() }).ToList());
            databasePermissionsList.Add(d,
                x2.Rows.Select(row => (object)new
                    {
                        ObjectName = row[0].ToString(),
                        Grantee = row[1].ToString(),
                        Permissions = row[2].ToString()
                    }).ToList());
        }
        response.Add("DatabaseRoles", JToken.FromObject(databaseRolesList));
        response.Add("DatabasePermissions", JToken.FromObject(databasePermissionsList));
        return new string(response.ToString().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    public Action DisconnectAll()
    {
        return () =>
        {
            foreach (var conn in _connections) _ = DeleteConnectionAsync(conn.Key);
        };
    }

    public void CheckIfConnectionExists(Guid guid)
    {
        if (_connections.ContainsKey(guid)) return;

        logger.LogError($"Connection wit id {guid} does not exist");
        throw new Exception($"Connection with id {guid} does not exist");
    }

    public async Task PerformLoadConnectionsAsync(DbSystemEntry connection)
    {
        if (_connections.TryGetValue(connection.Id, out var connection1))
        {
            if (connection1.State == ConnectionState.Open) return;

            _ = await DeleteConnectionAsync(connection.Id);
        }

        try
        {
            var conn = CreateSqlConnection(connection);
            await conn.OpenAsync();
            _connections.Add(connection.Id, conn);
            logger.LogInformation($"Connected to {connection.Address},{connection.Port}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Unable to connect to {connection.Address},{connection.Port}. {ex.Message}");
        }
    }

    public void ReadDbDynamicState(object? state)
    {
        _ = Task.Run(PerformReadDbDynamicStateAsync);
    }

    public void ReadDbStaticState(object? state)
    {
        _ = Task.Run(PerformReadDbStaticStateAsync);
    }

    public async Task PerformReadDbDynamicStateAsync()
    {
        foreach (var conn in _connections)
            _ = await _persistenceService.SaveDbDynamicStateAsync(await PerformReadDbDynamicStateAsync(conn.Key));
    }

    public async Task PerformReadDbStaticStateAsync()
    {
        foreach (var conn in _connections)
            _ = await _persistenceService.SaveDbStaticStateAsync(await PerformReadDbStaticStateAsync(conn.Key));
    }

    public async Task<bool> AddOrUpdateConnectionAsync(Guid guid)
    {
        var keyExists = _connections.ContainsKey(guid);
        if (keyExists) _ = await DeleteConnectionAsync(guid);

        var connections = await _persistenceService.GetBareDbSystemEntriesAsync(DbType.Mssql);
        var connection = connections?.FirstOrDefault(s => s.Id == guid);
        if (connection == null) return false;

        await PerformLoadConnectionsAsync(connection);
        return true;
    }

    public async Task<bool> DeleteConnectionAsync(Guid guid)
    {
        var deleted = false;
        var connToDelete = _connections.FirstOrDefault(c => c.Key == guid);
        if (connToDelete.Key == Guid.Empty) return deleted;
        await _semaphore.WaitAsync();
        try
        {
            await _connections[guid].CloseAsync();
            logger.LogInformation($"Disconnected from {connToDelete.Value.DataSource}");
        }
        catch (Exception)
        {
            logger.LogError($"Unable to disconnect from {connToDelete.Value.DataSource}");
        }
        finally
        {
            _ = _semaphore.Release();
        }

        _ = _connections.Remove(guid);
        deleted = true;
        return deleted;
    }

    public async Task<DbResult?> ExecuteQueryAsync(Guid connection, string query, string database = "master")
    {
        CheckIfConnectionExists(connection);
        DbResult result = new();
        await _semaphore.WaitAsync();
        try
        {
            _connections[connection].ChangeDatabase(database);
            await using SqlCommand cmd = new(query, _connections[connection]);
            await using var reader = await cmd.ExecuteReaderAsync();
            var i = 0;
            while (reader.Read())
            {
                i++;
                if (i == 1)
                    for (var j = 0; j < reader.FieldCount; j++)
                        result.Columns.Add(reader.GetName(j));

                List<string> row = new();

                for (var j = 0; j < reader.FieldCount; j++) row.Add(reader[j].ToString() ?? string.Empty);

                result.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            throw new Exception(ex.Message);
        }
        finally
        {
            _ = _semaphore.Release();
        }

        return result;
    }


    public async Task<DbDynamicState?> PerformReadDbDynamicStateAsync(Guid guid)
    {
        try
        {
            return new DbDynamicState
            {
                Id = Guid.NewGuid(),
                DbSystemEntryId = guid,
                Timestamp = DateTime.Now,
                ConnectionsCount = await GetConnectionsCountAsync(guid),
                DbAndTableSizesAsJson = await GetDbAndTableSizesAsJsonAsync(guid)
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<DbStaticState?> PerformReadDbStaticStateAsync(Guid guid)
    {
        try
        {
            return new DbStaticState
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                DbSystemEntryId = guid,
                Version = await GetVersionAsync(guid),
                MaxConnectionsCount = await GetMaxConnectionsCountAsync(guid)
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> CreateUserAsync(Guid guid, CreateRequest request)
    {
        if (request.Database == null) throw new Exception("Database property is missing.");

        await ExecuteNonQueryAsync(guid, $"CREATE USER {request.Name} FOR LOGIN {request.Name}", request.Database);
        return true;
    }

    public async Task<bool> CreateLoginAsync(Guid guid, CreateRequest request)
    {
        await ExecuteNonQueryAsync(guid, $"CREATE LOGIN {request.Name} WITH PASSWORD = '{request.Password}'");
        return true;
    }

    public async Task<bool> DropUserAsync(Guid guid, DropRequest request)
    {
        if (request.Database == null) throw new Exception("Database property is missing.");

        await ExecuteNonQueryAsync(guid, $"DROP USER {request.Name}", request.Database);
        return true;
    }

    public async Task<bool> DropLoginAsync(Guid guid, DropRequest request)
    {
        await ExecuteNonQueryAsync(guid, $"DROP LOGIN {request.Name}");
        return true;
    }

    public async Task<bool> GrantAsync(Guid guid, GrantOrRevokeRequest request)
    {
        if(request.Where is null)
            await ExecuteNonQueryAsync(guid, $"GRANT {request.What} TO {request.Who}", request.Database);    
        else
            await ExecuteNonQueryAsync(guid, $"GRANT {request.What} ON {request.Where} TO {request.Who}", request.Database);
        return true;
    }

    public async Task<bool> RevokeAsync(Guid guid, GrantOrRevokeRequest request)
    {
        if(request.Where is null)
            await ExecuteNonQueryAsync(guid, $"REVOKE {request.What} FROM {request.Who}");
        else
            await ExecuteNonQueryAsync(guid, $"REVOKE {request.What} ON {request.Where} FROM {request.Who}", request.Database);
        return true;
    }

    public void LoadConnections(object? state)
    {
        _ = Task.Run(LoadConnectionsAsync);
    }

    public async Task<bool> TestConnectivityAsync(Guid guid)
    {
        var result = await ExecuteQueryAsync(guid, "SELECT 1");
        try
        {
            var y = result?.Rows[0][0];
            return result != null && y == "1";
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Task<string?> ExecuteCommandAsync(Guid connection, string command)
    {
        throw new NotImplementedException();
    }

    public Task<OsDynamicState?> PerformReadOsDynamicStateAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task<OsStaticState?> PerformReadOsStaticStateAsync(Guid guid)
    {
        throw new NotImplementedException();
    }

    public Task PerformReadOsDynamicStateAsync()
    {
        throw new NotImplementedException();
    }

    public Task PerformReadOsStaticStateAsync()
    {
        throw new NotImplementedException();
    }

    public Task PerformLoadConnectionsAsync(OsSystemEntry connection)
    {
        throw new NotImplementedException();
    }

    public void ReadOsDynamicState(object? state)
    {
        throw new NotImplementedException();
    }

    public void ReadOsStaticState(object? state)
    {
        throw new NotImplementedException();
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

    public static SqlConnection CreateSqlConnection(DbSystemEntry connection)
    {
        SqlConnectionStringBuilder builder = new()
        {
            DataSource = connection.Port == 1443 ? $"{connection.Address}" : $"{connection.Address},{connection.Port}",
            UserID = connection.Username,
            Password = connection.Password,
            InitialCatalog = "master",
            ApplicationName = "DBG",
            TrustServerCertificate = true,
            Pooling = true,
            Encrypt = SqlConnectionEncryptOption.Optional,
            ConnectRetryCount = 0
        };
        return new SqlConnection(builder.ConnectionString);
    }

    public async Task<string> GetDbAndTableSizesAsJsonAsync(Guid guid)
    {
        JObject json = new();
        var databases = await ExecuteQueryAsync(guid, MSSQL.GetDatabases());
        if (databases == null) return string.Empty;

        foreach (var database in databases.Rows)
        {
            JObject nestedJson = new();
            var databaseName = database[0];
            var tableSizes = await ExecuteQueryAsync(guid, "EXEC sp_spaceused @oneresultset = 1", databaseName);
            var innerTableSizes = await ExecuteQueryAsync(guid, MSSQL.GetTablesSizes(), databaseName);
            if (tableSizes == null) continue;

            if (innerTableSizes == null) continue;

            nestedJson.Add("Total", tableSizes.Rows[0][1]);
            foreach (var row in innerTableSizes.Rows) nestedJson.Add(row[0], row[1]);

            json.Add(databaseName, nestedJson);
        }

        return new string(json.ToString().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    public async Task<int> GetConnectionsCountAsync(Guid guid)
    {
        var r = await ExecuteQueryAsync(guid, MSSQL.GetConnectionsCount());
        var s = r?.Rows[0][0];
        _ = int.TryParse(s, out var n);
        return n;
    }


    public async Task<string?> GetVersionAsync(Guid guid)
    {
        var r = await ExecuteQueryAsync(guid, "SELECT @@VERSION");
        var s = r?.Rows[0][0];
        return s;
    }

    public async Task<int> GetMaxConnectionsCountAsync(Guid guid)
    {
        var r = await ExecuteQueryAsync(guid, MSSQL.GetMaxConnectionsCount());
        var s = r?.Rows[0][0];
        _ = int.TryParse(s, out var n);
        return n;
    }
}