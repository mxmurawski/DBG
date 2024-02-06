#region

using DBG.Infrastructure.Models.Db;
using Microsoft.Extensions.Primitives;

#endregion

namespace DBG.Infrastructure.Interfaces;

public interface ILinuxClient
{
    Task CheckConnectivityAsync();
    Task<string?> ExecuteCommandAsync(Guid guid, string query, StringValues token);
    Task<bool> AddOrUpdateWorkerConnectionAsync(Guid guid, StringValues token);
    Task<bool> DeleteWorkerConnectionAsync(Guid guid, StringValues token);
    Task<bool> PingAsync();
    Task<OsStaticState?> GetStaticStateAsync(Guid guid, StringValues token);
    Task<OsDynamicState?> GetDynamicStateAsync(Guid guid, StringValues token);
    Task<bool> TestSystemConnectivityAsync(Guid guid, StringValues token);
}