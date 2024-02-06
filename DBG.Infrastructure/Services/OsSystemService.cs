#region

using DBG.Infrastructure.Enums;
using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using Microsoft.Extensions.Primitives;

#endregion

namespace DBG.Infrastructure.Services;

public class OsSystemService : IOsSystemService
{
    private readonly ILinuxClient linuxClient;
    private readonly IPersistenceService persistenceService;

    public OsSystemService(IPersistenceService persistenceService, ILinuxClient linuxClient)
    {
        this.persistenceService = persistenceService;
        this.linuxClient = linuxClient;
    }

    public async Task<bool> AddOrUpdateWorkerConnectionAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetOsSystemAttributesAsync(guid);
        return type switch
        {
            OsType.Linux => await linuxClient.AddOrUpdateWorkerConnectionAsync(id, token),
            _ => throw new Exception("Invalid OsType")
        };
    }

    public async Task<bool> DeleteWorkerConnectionAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetOsSystemAttributesAsync(guid);
        return type switch
        {
            OsType.Linux => await linuxClient.DeleteWorkerConnectionAsync(id, token),
            _ => throw new Exception("Invalid OsType")
        };
    }

    public Func<Task> DeleteWorkerConnection(Guid guid, StringValues token)
    {
        return () => DeleteWorkerConnectionAsync(guid, token);
    }

    public async Task<bool> TestOsSystemAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetOsSystemAttributesAsync(guid);
        return type switch
        {
            OsType.Linux => await linuxClient.TestSystemConnectivityAsync(id, token),
            _ => throw new Exception("Invalid OsType")
        };
    }

    public async Task<IList<OsStaticState>?> GetOsStaticStatesFromDbAsync(Guid guid, DateTime from, DateTime to)
    {
        var osEntryId = await persistenceService.GetSystemEntrysOsSystemsIdFromDbAsync(guid);
        return await persistenceService.GetOsStaticStatesFromDbAsync(osEntryId, from, to);
    }

    public async Task<IList<OsDynamicState>?> GetOsDynamicStatesFromDbAsync(Guid guid, DateTime from, DateTime to)
    {
        var osEntryId = await persistenceService.GetSystemEntrysOsSystemsIdFromDbAsync(guid);
        return await persistenceService.GetOsDynamicStatesFromDbAsync(osEntryId, from, to);
    }

    public async Task<OsDynamicState?> GetLastOsDynamicStateAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetOsSystemAttributesAsync(guid);
        return type switch
        {
            OsType.Linux => await linuxClient.GetDynamicStateAsync(id, token),
            _ => throw new Exception("Invalid OsType")
        };
    }

    public async Task<OsStaticState?> GetLastOsStaticStateAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetOsSystemAttributesAsync(guid);
        return type switch
        {
            OsType.Linux => await linuxClient.GetStaticStateAsync(id, token),
            _ => throw new Exception("Invalid OsType")
        };
    }

    public async Task<(Guid, OsType)> GetOsSystemAttributesAsync(Guid guid)
    {
        var osEntry = await persistenceService.GetOsSystemFromDbAsync(
            await persistenceService.GetSystemEntrysOsSystemsIdFromDbAsync(guid));
        return osEntry == null
            ? throw new Exception($"Unable to find os system for system with id {guid}")
            : (osEntry.Id, osEntry.OsType);
    }
}