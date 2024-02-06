#region

using System.Data;
using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Helper;
using DBG.Infrastructure.Models.Request;
using DBG.Infrastructure.Queries;
using Newtonsoft.Json.Linq;
using Npgsql;
using DbType = DBG.Infrastructure.Enums.DbType;

#endregion

namespace DBG.PostgresWorker;

public class ConnectionManager : IConnectionManager
{
    private readonly Dictionary<Guid, NpgsqlConnection> _connections;
    private readonly IPersistenceService _persistenceService;
    private readonly SemaphoreSlim _semaphore;
    private readonly Dictionary<Guid, Dictionary<string, NpgsqlConnection>> _subconnections;
    private readonly ILogger<ConnectionManager> logger;

    public ConnectionManager(ILogger<ConnectionManager> logger, IServiceScopeFactory scopeFactory)
    {
        this.logger = logger;
        _persistenceService = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IPersistenceService>();
        _semaphore = new SemaphoreSlim(1);
        _connections = new Dictionary<Guid, NpgsqlConnection>();
        _subconnections = new Dictionary<Guid, Dictionary<string, NpgsqlConnection>>();
    }

    public async Task<string?> GetPermissionAuditResultAsync(Guid guid)
    {
        JObject response = new();
        var serverRoles = await ExecuteQueryAsync(guid, Postgres.GetServerRoles());
        var serverRolesList = serverRoles?.Rows.Select(row => (object)new
        {
            Role = row[0].ToString(),
            Members = row[1].ToString()
        }).ToList();
        if (serverRolesList == null) return null;
        response.Add("ServerRoles", JToken.FromObject(serverRolesList));
        var databases = await ExecuteQueryAsync(guid, Postgres.GetDatabases());
        var databasesList = databases?.Rows.Select(row => row[0].ToString()).ToList();
        Dictionary<string, List<object>> databasePermissionsList = new();
        if (databasesList == null) return null;
        foreach (var d in databasesList)
        {
            var x = await ExecuteQueryAsync(guid, Postgres.GetDatabasePermissionsList(), d);
            if (x == null) continue;
            databasePermissionsList.Add(d,
                x.Rows.Select(row => (object)new
                    {
                        ObjectName = row[0].ToString(),
                        Grantee = row[1].ToString(),
                        Permissions = row[2].ToString()
                    }).ToList());
        }
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
        throw new NotImplementedException();
    }

    public void CheckIfConnectionExists(Guid guid, string database = "postgres")
    {
        if (
            (database != "postgres" && _connections.ContainsKey(guid) && _subconnections[guid].ContainsKey(database)) ||
            (database == "postgres" && _connections.ContainsKey(guid))
        ) return;
        throw new Exception($"Connection with id {guid} for database {database} does not exist");
    }

    public async Task ConnectToNewDatabasesAsync()
    {
        var connections = await _persistenceService.GetBareDbSystemEntriesAsync(DbType.Postgres);
        if (connections == null) return;
        foreach (var conn in _connections)
        {
            var newDatabaseRows = (await ExecuteQueryAsync(conn.Key, Postgres.GetDatabases()))?.Rows;
            var newDatabases = newDatabaseRows?.Select(databaseRow => databaseRow[0]).ToList();
            var oldDatabases = _subconnections[conn.Key].Keys.ToList();
            var addedDatabases = newDatabases?.Except(oldDatabases).ToList();
            var deletedDatabases = oldDatabases.Except(newDatabases ?? Enumerable.Empty<string>()).ToList();
            if (addedDatabases is not null)
                foreach (var newDatabase in addedDatabases)
                {
                    var connection = connections.FirstOrDefault(s => s.Id == conn.Key);
                    if (connection == null) continue;
                    try
                    {
                        var subconn = CreateNpgsqlConnection(connection, newDatabase);
                        await subconn.OpenAsync();
                        _subconnections[conn.Key].Add(newDatabase, subconn);
                        logger.LogInformation($"Connected to database {newDatabase} at {connection.Address}:{connection.Port}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Unable to connect to database {newDatabase} at {connection.Address}:{connection.Port}. {ex.Message}");
                    }
                }
            foreach (var deletedDatabase in deletedDatabases)
            {
                var connToDelete = _subconnections[conn.Key][deletedDatabase];
                try
                {
                    await connToDelete.CloseAsync();
                    logger.LogInformation($"Disconnected from {deletedDatabase} at {conn.Value.DataSource}");
                }
                catch (Exception)
                {
                    logger.LogError($"Unable to disconnect from {deletedDatabase} at {conn.Value.DataSource}");
                }
                _subconnections[conn.Key].Remove(deletedDatabase);
            }
        }
    }

    public async Task LoadConnectionsAsync()
    {
        var connections = await _persistenceService.GetBareDbSystemEntriesAsync(DbType.Postgres);
        if (connections != null)
            foreach (var connection in connections)
                await PerformLoadConnectionsAsync(connection);
    }

    public async Task PerformLoadConnectionsAsync(DbSystemEntry connection)
    {
        if (_connections.TryGetValue(connection.Id, out var connection1))
        {
            if (connection1.State == ConnectionState.Open) return;
            await DeleteConnectionAsync(connection.Id);
        }
        try
        {
            var conn = CreateNpgsqlConnection(connection);
            await conn.OpenAsync();
            _connections.Add(connection.Id, conn);
            logger.LogInformation($"Connected to database postgres at {connection.Address}:{connection.Port}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Unable to connect to database postgres at {connection.Address}:{connection.Port}. {ex.Message}");
            return;
        }
        var databaseRows = (await ExecuteQueryAsync(connection.Id, Postgres.GetDatabases()))?.Rows;
        var databases = databaseRows?.Select(databaseRow => databaseRow[0]).ToList();
        if (databases == null) return;
        Dictionary<string, NpgsqlConnection> subconns = new();
        foreach (var database in databases)
            try
            {
                var subconn = CreateNpgsqlConnection(connection, database);
                await subconn.OpenAsync();
                subconns.Add(database, subconn);
                logger.LogInformation($"Connected to database {database} at {connection.Address}:{connection.Port}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Unable to connect to database {database} at {connection.Address}:{connection.Port}. {ex.Message}");
            }
        _subconnections.Add(connection.Id, subconns);
    }

    public void ReadDbDynamicState(object? state)
    {
        Task.Run(PerformReadDbDynamicStateAsync);
    }

    public void LoadConnections(object? state)
    {
        Task.Run(LoadConnectionsAsync);
    }

    public void ConnectToNewDatabases(object? state)
    {
        Task.Run(ConnectToNewDatabasesAsync);
    }

    public void ReadDbStaticState(object? state)
    {
        Task.Run(PerformReadDbStaticStateAsync);
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

    public async Task<DbDynamicState?> PerformReadDbDynamicStateAsync(Guid guid)
    {
        try
        {
            return new DbDynamicState
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                DbSystemEntryId = guid,
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
        await ExecuteNonQueryAsync(guid, "CREATE USER " + request.Name + " WITH PASSWORD '" + request.Password + "'");
        return true;
    }

    public Task<bool> CreateLoginAsync(Guid guid, CreateRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DropUserAsync(Guid guid, DropRequest request)
    {
        await ExecuteNonQueryAsync(guid, "DROP USER " + request.Name);
        return true;
    }

    public Task<bool> DropLoginAsync(Guid guid, DropRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> GrantAsync(Guid guid, GrantOrRevokeRequest request)
    {
        if(request.Where is null)
            await ExecuteNonQueryAsync(guid, $"GRANT {request.What} TO {request.Who}", request.Database);
        else
            await ExecuteNonQueryAsync(guid, "GRANT " + request.What + " ON " + request.Where + " TO " + request.Who, request.Database);
        return true;
    }

    public async Task<bool> RevokeAsync(Guid guid, GrantOrRevokeRequest request)
    {
        if(request.Where is null)
            await ExecuteNonQueryAsync(guid, $"REVOKE {request.What} FROM {request.Who}");
        else
            await ExecuteNonQueryAsync(guid, "REVOKE " + request.What + " ON " + request.Where + " FROM " + request.Who, request.Database);
        return true;
    }

    public async Task<bool> AddOrUpdateConnectionAsync(Guid guid)
    {
        var keyExists = _connections.ContainsKey(guid);
        if (keyExists) await DeleteConnectionAsync(guid);
        var connections = await _persistenceService.GetBareDbSystemEntriesAsync(DbType.Postgres);
        var connection = connections?.FirstOrDefault(s => s.Id == guid);
        if (connection == null) return false;
        await PerformLoadConnectionsAsync(connection);
        return true;
    }

    public async Task<bool> DeleteConnectionAsync(Guid guid)
    {
        var connToDelete = _connections.FirstOrDefault(c => c.Key == guid);
        if (connToDelete.Value is not null)
        {
            await _semaphore.WaitAsync();
            (string host, int port) = (connToDelete.Value.Host!, connToDelete.Value.Port);
            try
            {
                await connToDelete.Value.CloseAsync();
                logger.LogInformation($"Disconnected from postgres at {host}:{port}");
            }
            catch (Exception)
            {
                logger.LogError($"Unable to disconnect from postgres at {host}:{port}");
            }
            finally
            {
                _ = _semaphore.Release();
            }
            _ = _connections.Remove(guid);
        }
        var connsToDelete = _subconnections.FirstOrDefault(c => c.Key == guid);
        if (connsToDelete.Value.Count == 0) return true;
        {
            foreach (var c in connsToDelete.Value)
            {
                (string host, int port) = (c.Value.Host!, c.Value.Port);
                try
                {
                    await c.Value.CloseAsync();
                    logger.LogInformation($"Disconnected from {c.Key} at {host}:{port}");
                }
                catch (Exception)
                {
                    logger.LogError($"Unable to disconnect from {c.Key} at {host}:{port}");
                }
            }
            _subconnections.Remove(guid);
        }
        return true;
    }

    public async Task ExecuteNonQueryAsync(Guid connection, string query)
    {
        await ExecuteNonQueryAsync(connection, query, "postgres");
    }

    public async Task<DbResult?> ExecuteQueryAsync(Guid connection, string query, string database = "postgres")
    {
        CheckIfConnectionExists(connection, database);
        DbResult? result;
        await _semaphore.WaitAsync();
        try
        {
            var conToUse = database == "postgres" ? _connections[connection] : _subconnections[connection][database];
            await using NpgsqlCommand cmd = new(query, conToUse);
            await using var reader = await cmd.ExecuteReaderAsync();
            var i = 0;
            result = new DbResult();
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

    public async Task ExecuteNonQueryAsync(Guid connection, string query, string database = "postgres")
    {
        CheckIfConnectionExists(connection, database);
        await _semaphore.WaitAsync();
        try
        {
            var conToUse = database == "postgres" ? _connections[connection] : _subconnections[connection][database];
            await using NpgsqlCommand cmd = new(query, conToUse);
            await cmd.ExecuteNonQueryAsync();
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

    public async Task<bool> TestConnectivityAsync(Guid guid)
    {
        var result = await ExecuteQueryAsync(guid, "SELECT 1");
        try
        {
            var y = result?.Rows[0][0];
            return result != null && y == "1";
        }
        catch
        {
            return false;
        }
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

    public Task PerformReadOsDynamicStateAsync()
    {
        throw new NotImplementedException();
    }

    public Task PerformReadOsStaticStateAsync()
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

    public Task<string?> ExecuteCommandAsync(Guid connection, string command)
    {
        throw new NotImplementedException();
    }

    public static NpgsqlConnection CreateNpgsqlConnection(DbSystemEntry connection, string database = "postgres")
    {
        NpgsqlConnectionStringBuilder builder = new()
        {
            Host = connection.Address,
            Port = connection.Port,
            Username = connection.Username,
            Password = connection.Password,
            Database = database,
            SslMode = SslMode.Allow,
            ApplicationName = "DBG",
            Pooling = true,
            KeepAlive = 60
        };
        return new NpgsqlConnection(builder.ConnectionString);
    }

    public async Task<string?> GetDbAndTableSizesAsJsonAsync(Guid guid)
    {
        JObject json = new();
        var databasesAndSizes = await ExecuteQueryAsync(guid, Postgres.GetDatabaseSizes());
        if (databasesAndSizes == null) return null;
        foreach (var readRow in databasesAndSizes.Rows)
        {
            JObject nestedJson = new();
            var databaseName = readRow[0];
            nestedJson.Add("Total", readRow[1]);
            var tableSizes = await ExecuteQueryAsync(guid,
                Postgres.GetTablesSizes(),
                databaseName);
            if (tableSizes == null) continue;
            foreach (var row in tableSizes.Rows) nestedJson.Add(row[0], row[1]);
            json.Add(databaseName, nestedJson);
        }
        return new string(json.ToString().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    public async Task<int> GetConnectionsCountAsync(Guid guid)
    {
        var r = await ExecuteQueryAsync(guid, Postgres.GetConnectionsCount());
        var s = r?.Rows[0][0];
        int.TryParse(s, out var n);
        return n;
    }

    public async Task<string?> GetVersionAsync(Guid guid)
    {
        var r = await ExecuteQueryAsync(guid, "SHOW server_version");
        var s = r?.Rows[0][0];
        return s;
    }

    public async Task<int> GetMaxConnectionsCountAsync(Guid guid)
    {
        var r = await ExecuteQueryAsync(guid, "SHOW max_connections");
        var s = r?.Rows[0][0];
        int.TryParse(s, out var n);
        return n;
    }
}