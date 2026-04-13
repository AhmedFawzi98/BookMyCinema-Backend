------------------ Logs table
-- Timestamp index (for time based filtering)
CREATE INDEX IX_HttpLogs_Timestamp
ON Logging.Logs(TimeStamp DESC);


------------------ HttpLogs table
-- Combined Path And StatusCode (Covers Path + (Path + StatusCode)) querying
CREATE INDEX IX_HttpLogs_Path_StatusCode 
ON Logging.HttpLogs(Path, StatusCode);

-- StatusCode (error analysis)
CREATE INDEX IX_HttpLogs_StatusCode ON Logging.HttpLogs(StatusCode);

-- Filtered index for UserId 
CREATE INDEX IX_HttpLogs_UserId_NotNull
ON Logging.HttpLogs(UserId)
WHERE UserId IS NOT NULL;

-- Timestamp index (for time based filtering)
CREATE INDEX IX_HttpLogs_Timestamp
ON Logging.HttpLogs(TimeStamp DESC);
