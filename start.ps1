(Get-Content -Raw -Path .\DBG.API\s.sh) -replace "`r`n", "`n" | Set-Content -Path .\DBG.API\s.sh
(Get-Content -Raw -Path .\DBG.MSSQLWorker\s.sh) -replace "`r`n", "`n" | Set-Content -Path .\DBG.MSSQLWorker\s.sh
(Get-Content -Raw -Path .\DBG.PostgresWorker\s.sh) -replace "`r`n", "`n" | Set-Content -Path .\DBG.PostgresWorker\s.sh
(Get-Content -Raw -Path .\DBG.SSHWorker\s.sh) -replace "`r`n", "`n" | Set-Content -Path .\DBG.SSHWorker\s.sh
(Get-Content -Raw -Path .\vault-deploy.sh) -replace "`r`n", "`n" | Set-Content -Path .\vault-deploy.sh
(Get-Content -Raw -Path .\db-deploy.sh) -replace "`r`n", "`n" | Set-Content -Path .\db-deploy.sh
docker-compose up -d --build