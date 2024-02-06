namespace DBG.Infrastructure.Queries;

public class Postgres
{
    public static string GetServerRoles()
    {
        return
            "SELECT r.rolname AS \"Database Role\", string_agg(m.rolname, ', ') AS \"Members\" FROM pg_roles r LEFT JOIN pg_auth_members am ON r.oid = am.roleid LEFT JOIN pg_roles m ON am.member = m.oid GROUP BY r.rolname ORDER BY r.rolname";
    }

    public static string GetDatabases()
    {
        return "select datname from pg_database where datname not like 'postgres' and datname not like 'template%'";
    }

    public static string GetDatabasePermissionsList()
    {
        return
            "SELECT table_catalog || '.' || table_schema || '.' || table_name as obj_name, grantee, string_agg(privilege_type,', ') FROM information_schema.table_privileges WHERE grantee not in ('postgres', 'PUBLIC') group by obj_name, grantee";
    }

    public static string GetDatabaseSizes()
    {
        return
            "SELECT datname AS database_name, pg_size_pretty(pg_database_size(datname)) AS total_size FROM pg_database WHERE datname NOT LIKE 'template%' AND datname != 'postgres'";
    }

    public static string GetTablesSizes()
    {
        return
            "SELECT table_schema || '.' || table_name AS table_name, pg_size_pretty(pg_total_relation_size(table_schema || '.' || table_name)) AS total_size FROM information_schema.tables WHERE table_schema NOT LIKE 'pg_catalog' AND table_schema NOT LIKE 'information_schema' AND table_type NOT LIKE 'VIEW'";
    }

    public static string GetTables()
    {
        return
            "SELECT table_schema || '.' || table_name FROM information_schema.tables where table_schema not in ('pg_catalog', 'information_schema')";
    }

    public static string GetConnectionsCount()
    {
        return "SELECT count(*) FROM pg_stat_activity WHERE state is not null";
    }
}