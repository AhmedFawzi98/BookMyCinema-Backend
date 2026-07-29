------------------ Logs table
-- Timestamp index (for time based filtering)
CREATE INDEX IX_HttpLogs_Timestamp
ON Logging.Logs(TimeStamp)
INCLUDE (Level, Message);

------------------ HttpLogs table
-- Filtered index for UserId 
CREATE INDEX IX_HttpLogs_UserId_NotNull
ON Logging.HttpLogs(UserId)
WHERE UserId IS NOT NULL;

-- for recent errors queries
CREATE INDEX IX_HttpLogs_Errors_Timestamp
ON Logging.HttpLogs(TimeStamp DESC)
INCLUDE (StatusCode, Path)
WHERE StatusCode >= 400;
