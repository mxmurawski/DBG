if [ ! -f /vault/initialized ]
then
    sleep 120
fi
export VAULTTOKEN=$(</vault/rt)
dotnet DBG.MSSQLWorker.dll