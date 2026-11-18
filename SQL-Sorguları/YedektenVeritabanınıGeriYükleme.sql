RESTORE DATABASE store5
FROM DISK = N'/var/opt/mssql/backup/store5.bak'
WITH MOVE N'store5'     TO N'/var/opt/mssql/data/store5.mdf',
     MOVE N'store5_log' TO N'/var/opt/mssql/data/store5_log.ldf',
     REPLACE, STATS = 10;