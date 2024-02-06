#region

using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Helper;
using DBG.Infrastructure.Models.Request;

#endregion

namespace DBG.Infrastructure.Interfaces;

public interface IConnectionManager
{
    Task<bool> TestConnectivityAsync(Guid guid);
    Task<string?> GetPermissionAuditResultAsync(Guid guid);
    Action DisconnectAll();
    void LoadConnections(object? state);
    void CheckIfConnectionExists(Guid guid);
    void CheckIfConnectionExists(Guid guid, string database);
    void ConnectToNewDatabases(object? state);
    Task ConnectToNewDatabasesAsync();
    Task PerformLoadConnectionsAsync(DbSystemEntry connection);
    Task PerformLoadConnectionsAsync(OsSystemEntry connection);
    Task<bool> AddOrUpdateConnectionAsync(Guid guid);
    Task<bool> DeleteConnectionAsync(Guid guid);
    Task ExecuteNonQueryAsync(Guid connection, string query);
    Task<string?> ExecuteCommandAsync(Guid connection, string command);
    Task<DbResult?> ExecuteQueryAsync(Guid connection, string query, string database);
    Task ExecuteNonQueryAsync(Guid connection, string query, string database);
    Task LoadConnectionsAsync();
    void ReadOsDynamicState(object? state);
    void ReadOsStaticState(object? state);
    void ReadDbDynamicState(object? state);
    void ReadDbStaticState(object? state);
    Task PerformReadOsDynamicStateAsync();
    Task PerformReadOsStaticStateAsync();
    Task PerformReadDbDynamicStateAsync();
    Task PerformReadDbStaticStateAsync();
    Task<OsDynamicState?> PerformReadOsDynamicStateAsync(Guid guid);
    Task<OsStaticState?> PerformReadOsStaticStateAsync(Guid guid);
    Task<DbDynamicState?> PerformReadDbDynamicStateAsync(Guid guid);
    Task<DbStaticState?> PerformReadDbStaticStateAsync(Guid guid);
    Task<bool> CreateUserAsync(Guid guid, CreateRequest request);
    Task<bool> CreateLoginAsync(Guid guid, CreateRequest request);
    Task<bool> DropUserAsync(Guid guid, DropRequest request);
    Task<bool> DropLoginAsync(Guid guid, DropRequest request);
    Task<bool> GrantAsync(Guid guid, GrantOrRevokeRequest request);
    Task<bool> RevokeAsync(Guid guid, GrantOrRevokeRequest request);
}