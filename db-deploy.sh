/opt/mssql/bin/sqlservr 2>&1 &
sleep 60
if [ ! -f /opt/mssql/db-initialized ]
then
    /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -i /opt/mssql/migration.sql -C
    touch /opt/mssql/db-initialized
fi
tail -f /dev/null