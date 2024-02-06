# Pronouncement

This project is an application, that is intended to be a part of my dissertation. Improved version of this software is being developed and will be published at https://github.com/mxmurawski/DBG2.

# Prerequisites

You have to have installed and started Docker on your machine.

# How to run?

Ensure that all *.sh files have LF line endings and run below command:<br><br><i>docker-compose up -d --build</i>

Alternatively You can run start.ps1 Powershell script file which automatically convert all *.sh files' line endings from
CRLF to LF.

# Credentials to test server:

ip: 20.107.245.249\
universal username: collegiumdavinci<br>
universal password: collegiumdavinci123!<br>

port 5432 - Postgres<br>
port 1443 - MSSQL<br>
port 22 - SSH<br>

# Automatically created app users

<table>
<tr><td>username</td><td>password</td></tr>
<tr><td>admin@admin.com</td><td>admin</td></tr>
<tr><td>viewer@admin.com</td><td>viewer</td></tr>
</table>