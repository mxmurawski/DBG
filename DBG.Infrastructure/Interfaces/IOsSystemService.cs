#region

using DBG.Infrastructure.Enums;
using DBG.Infrastructure.Models.Db;
using Microsoft.Extensions.Primitives;

#endregion

namespace DBG.Infrastructure.Interfaces;

public interface IOsSystemService
{
    Task<bool> AddOrUpdateWorkerConnectionAsync(Guid guid, StringValues token);
    Task<bool> DeleteWorkerConnectionAsync(Guid guid, StringValues token);
    Task<bool> TestOsSystemAsync(Guid guid, StringValues token);
    Task<IList<OsStaticState>?> GetOsStaticStatesFromDbAsync(Guid guid, DateTime from, DateTime to);
    Task<IList<OsDynamicState>?> GetOsDynamicStatesFromDbAsync(Guid guid, DateTime from, DateTime to);
    Task<OsDynamicState?> GetLastOsDynamicStateAsync(Guid guid, StringValues token);
    Task<OsStaticState?> GetLastOsStaticStateAsync(Guid guid, StringValues token);
    Task<(Guid, OsType)> GetOsSystemAttributesAsync(Guid guid);
}