#region

using DBG.Infrastructure.Enums;
using DBG.Infrastructure.Interfaces;
using DBG.Infrastructure.Models.Db;
using DBG.Infrastructure.Models.Request;
using Microsoft.Extensions.Primitives;

#endregion

namespace DBG.Infrastructure.Services;

public class DbSystemService : IDbSystemService
{
    private readonly IMssqlClient mssqlClient;
    private readonly IPersistenceService persistenceService;
    private readonly IPostgresClient postgresClient;

    public DbSystemService(
        IPersistenceService persistenceService,
        IPostgresClient postgresClient,
        IMssqlClient mssqlClient)
    {
        this.persistenceService = persistenceService;
        this.postgresClient = postgresClient;
        this.mssqlClient = mssqlClient;
    }

    public async Task<bool> AddOrUpdateWorkerConnectionAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => await postgresClient.AddOrUpdateWorkerConnectionAsync(id, token),
            DbType.Mssql => await mssqlClient.AddOrUpdateWorkerConnectionAsync(id, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public async Task<bool> DeleteWorkerConnectionAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => await postgresClient.DeleteWorkerConnectionAsync(id, token),
            DbType.Mssql => await mssqlClient.DeleteWorkerConnectionAsync(id, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public Func<Task> DeleteWorkerConnection(Guid guid, StringValues token)
    {
        return () => DeleteWorkerConnectionAsync(guid, token);
    }

    public async Task<bool> TestDbSystemAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => await postgresClient.TestSystemConnectivityAsync(id, token),
            DbType.Mssql => await mssqlClient.TestSystemConnectivityAsync(id, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public async Task<IList<DbStaticState>?> GetDbStaticStatesFromDbAsync(Guid guid, DateTime from, DateTime to)
    {
        var dbEntryId = await persistenceService.GetSystemEntrysDbSystemsIdFromDbAsync(guid);
        return await persistenceService.GetDbStaticStatesFromDbAsync(dbEntryId, from, to);
    }

    public async Task<IList<DbDynamicState>?> GetDbDynamicStatesFromDbAsync(Guid guid, DateTime from, DateTime to)
    {
        var dbEntryId = await persistenceService.GetSystemEntrysDbSystemsIdFromDbAsync(guid);
        return await persistenceService.GetDbDynamicStatesFromDbAsync(dbEntryId, from, to);
    }

    public async Task<DbDynamicState?> GetLastDbDynamicStateAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => await postgresClient.GetDynamicStateAsync(id, token),
            DbType.Mssql => await mssqlClient.GetDynamicStateAsync(id, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public async Task<DbStaticState?> GetLastDbStaticStateAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => await postgresClient.GetStaticStateAsync(id, token),
            DbType.Mssql => await mssqlClient.GetStaticStateAsync(id, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public async Task<string?> GetPermissionAuditResultAsync(Guid guid, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => await postgresClient.GetPermissionAuditResultAsync(id, token),
            DbType.Mssql => await mssqlClient.GetPermissionAuditResultAsync(id, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public async Task<(Guid, DbType)> GetDbSystemAttributesAsync(Guid guid)
    {
        var dbEntry = await persistenceService.GetDbSystemFromDbAsync(
            await persistenceService.GetSystemEntrysDbSystemsIdFromDbAsync(guid));
        return dbEntry == null
            ? throw new Exception($"Unable to find database system for system with id {guid}")
            : (dbEntry.Id, dbEntry.DbType);
    }

    public async Task<bool> CreateUserAsync(Guid guid, CreateRequest request, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => await postgresClient.CreateUserAsync(id, request, token),
            DbType.Mssql => await mssqlClient.CreateUserAsync(id, request, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public async Task<bool> CreateLoginAsync(Guid guid, CreateRequest request, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => throw new Exception("Not supported"),
            DbType.Mssql => await mssqlClient.CreateLoginAsync(id, request, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public async Task<bool> DropUserAsync(Guid guid, DropRequest request, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => await postgresClient.DropUserAsync(id, request, token),
            DbType.Mssql => await mssqlClient.DropUserAsync(id, request, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public async Task<bool> DropLoginAsync(Guid guid, DropRequest request, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => throw new Exception("Not supported"),
            DbType.Mssql => await mssqlClient.DropLoginAsync(id, request, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public async Task<bool> GrantAsync(Guid guid, GrantOrRevokeRequest request, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => await postgresClient.GrantAsync(id, request, token),
            DbType.Mssql => await mssqlClient.GrantAsync(id, request, token),
            _ => throw new Exception("Invalid DbType")
        };
    }

    public async Task<bool> RevokeAsync(Guid guid, GrantOrRevokeRequest request, StringValues token)
    {
        (var id, var type) = await GetDbSystemAttributesAsync(guid);
        return type switch
        {
            DbType.Postgres => await postgresClient.RevokeAsync(id, request, token),
            DbType.Mssql => await mssqlClient.RevokeAsync(id, request, token),
            _ => throw new Exception("Invalid DbType")
        };
    }
}