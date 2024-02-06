namespace DBG.Infrastructure.Queries;

public class MSSQL
{
    public static string GetMaxConnectionsCount()
    {
        return "SELECT value FROM sys.configurations WHERE NAME = 'user connections'";
    }

    public static string GetConnectionsCount()
    {
        return "SELECT COUNT(*) FROM sys.dm_exec_connections WHERE session_id > 0";
    }

    public static string GetDatabases()
    {
        return "select name FROM sys.databases where database_id>4";
    }

    public static string GetTablesSizes()
    {
        return
            "SELECT s.name + '.' + t.name as [Table], CAST(SUM(a.total_pages)*8 AS VARCHAR)+'kB' AS Size FROM sys.tables t INNER JOIN sys.indexes i ON t.object_id = i.object_id INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id LEFT JOIN sys.schemas s ON t.schema_id = s.schema_id GROUP BY t.name, s.name order by s.name, t.name";
    }

    public static string GetServerRoles()
    {
        return
            "SELECT r.name AS 'Server Role', ISNULL(STRING_AGG(p.name, ', '), null) AS 'Members' FROM sys.server_principals r LEFT JOIN sys.server_role_members srm ON r.principal_id = srm.role_principal_id LEFT JOIN sys.server_principals p ON srm.member_principal_id = p.principal_id WHERE r.type='R' GROUP BY r.name ORDER BY r.name";
    }

    public static string GetDatabaseRoles()
    {
        return
            "SELECT r.name AS 'Database Role', ISNULL(STRING_AGG(p.name, ', '), 'No members') AS 'Members' FROM sys.database_role_members drm JOIN sys.database_principals r ON drm.role_principal_id = r.principal_id LEFT JOIN sys.database_principals p ON drm.member_principal_id = p.principal_id GROUP BY r.name ORDER BY r.name;";
    }

    public static string GetDatabasePermissionsList()
    {
        return
            "select case when dpm.class_desc like 'DATABASE' THEN DB_NAME() when dpm.class_desc like 'OBJECT_OR_COLUMN' THEN CONCAT(DB_NAME(),'.', SCHEMA_NAME(OBJECTPROPERTY(dpm.major_id, 'SchemaId')),'.',OBJECT_NAME(dpm.major_id)) end as [ObjectName], dp.name as [Grantee], STRING_AGG(dpm.permission_name,', ') AS PermissionName FROM sys.database_permissions dpm JOIN sys.database_principals dp ON dpm.grantee_principal_id = dp.principal_id WHERE dpm.major_id >= 0 and state_desc='GRANT' group by case when dpm.class_desc like 'DATABASE' THEN DB_NAME() when dpm.class_desc like 'OBJECT_OR_COLUMN' THEN CONCAT(DB_NAME(),'.', SCHEMA_NAME(OBJECTPROPERTY(dpm.major_id, 'SchemaId')),'.',OBJECT_NAME(dpm.major_id)) end, dp.name";
    }

    public static string GetTables()
    {
        return
            "SELECT s.name + '.' + t.name FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id order by s.name, t.name";
    }
}