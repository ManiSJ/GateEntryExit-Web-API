RESTORE DATABASE [GateEntryExit]
FROM DISK = '/var/opt/mssql/backup/GateEntryExit.bak'
WITH 
    MOVE 'GateEntryExit' TO '/var/opt/mssql/data/GateEntryExit.mdf',
    MOVE 'GateEntryExit_log' TO '/var/opt/mssql/data/GateEntryExit_log.ldf',
    REPLACE,
    RECOVERY;