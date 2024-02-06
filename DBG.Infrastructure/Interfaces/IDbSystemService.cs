#region

using DBG.Infrastructure.Enums;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Request;
using Microsoft.Extensions.Primitives;

#endregion

namespace DBG.Infrastructure.Interfaces;

public interface IDbSystemService
{
    Task<bool> AddOrUpdateWorkerConnectionAsync(Guid guid, StringValues token);
    Task<bool> DeleteWorkerConnectionAsync(Guid guid, StringValues token);
    Task<bool> TestDbSystemAsync(Guid guid, StringValues token);
    Task<IList<DbStaticState>?> GetDbStaticStatesFromDbAsync(Guid guid, DateTime from, DateTime to);
    Task<IList<DbDynamicState>?> GetDbDynamicStatesFromDbAsync(Guid guid, DateTime from, DateTime to);
    Task<DbDynamicState?> GetLastDbDynamicStateAsync(Guid guid, StringValues token);
    Task<DbStaticState?> GetLastDbStaticStateAsync(Guid guid, StringValues token);
    Task<string?> GetPermissionAuditResultAsync(Guid guid, StringValues token);
    Task<(Guid, DbType)> GetDbSystemAttributesAsync(Guid guid);
    Task<bool> CreateUserAsync(Guid guid, CreateRequest request, StringValues token);
    Task<bool> CreateLoginAsync(Guid guid, CreateRequest request, StringValues token);
    Task<bool> DropUserAsync(Guid guid, DropRequest request, StringValues token);
    Task<bool> DropLoginAsync(Guid guid, DropRequest request, StringValues token);
    Task<bool> GrantAsync(Guid guid, GrantOrRevokeRequest request, StringValues token);
    Task<bool> RevokeAsync(Guid guid, GrantOrRevokeRequest request, StringValues token);
}