#region

using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Helper;
using DBG.Infrastructure.Models.Request;
using Microsoft.Extensions.Primitives;

#endregion

namespace DBG.Infrastructure.Interfaces;

public interface IPostgresClient
{
    Task CheckConnectivityAsync();
    Task<DbResult?> ExecuteQueryAsync(Guid guid, string query, StringValues token, string database = "postgres");
    Task<int> ExecuteNonQueryAsync(Guid guid, string query, StringValues token, string database = "postgres");
    Task<bool> AddOrUpdateWorkerConnectionAsync(Guid guid, StringValues token);
    Task<bool> DeleteWorkerConnectionAsync(Guid guid, StringValues token);
    Task<bool> PingAsync();
    Task<DbStaticState?> GetStaticStateAsync(Guid guid, StringValues token);
    Task<DbDynamicState?> GetDynamicStateAsync(Guid guid, StringValues token);
    Task<string?> GetPermissionAuditResultAsync(Guid guid, StringValues token);
    Task<bool> TestSystemConnectivityAsync(Guid guid, StringValues token);
    Task<bool> CreateUserAsync(Guid guid, CreateRequest request, StringValues token);
    Task<bool> DropUserAsync(Guid guid, DropRequest request, StringValues token);
    Task<bool> GrantAsync(Guid guid, GrantOrRevokeRequest request, StringValues token);
    Task<bool> RevokeAsync(Guid guid, GrantOrRevokeRequest request, StringValues token);
}