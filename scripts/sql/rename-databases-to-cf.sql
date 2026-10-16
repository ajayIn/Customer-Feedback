-- Run in SSMS connected to AJAYPC (Windows auth).
-- Renames v1 databases to CF_* names. Stop the three APIs first if they hold connections.

USE master;
GO

IF DB_ID(N'IdentityDb') IS NOT NULL AND DB_ID(N'CF_Identity') IS NULL
BEGIN
    ALTER DATABASE [IdentityDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE [IdentityDb] MODIFY NAME = [CF_Identity];
    ALTER DATABASE [CF_Identity] SET MULTI_USER;
    PRINT 'Renamed IdentityDb -> CF_Identity';
END
ELSE IF DB_ID(N'CF_Identity') IS NOT NULL
    PRINT 'CF_Identity already exists.';
ELSE
    PRINT 'IdentityDb not found; create CF_Identity manually if needed.';
GO

IF DB_ID(N'FeedbackDb') IS NOT NULL AND DB_ID(N'CF_Feedback') IS NULL
BEGIN
    ALTER DATABASE [FeedbackDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE [FeedbackDb] MODIFY NAME = [CF_Feedback];
    ALTER DATABASE [CF_Feedback] SET MULTI_USER;
    PRINT 'Renamed FeedbackDb -> CF_Feedback';
END
ELSE IF DB_ID(N'CF_Feedback') IS NOT NULL
    PRINT 'CF_Feedback already exists.';
ELSE
    PRINT 'FeedbackDb not found; create CF_Feedback manually if needed.';
GO

IF DB_ID(N'ReportingDb') IS NOT NULL AND DB_ID(N'CF_Reporting') IS NULL
BEGIN
    ALTER DATABASE [ReportingDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ALTER DATABASE [ReportingDb] MODIFY NAME = [CF_Reporting];
    ALTER DATABASE [CF_Reporting] SET MULTI_USER;
    PRINT 'Renamed ReportingDb -> CF_Reporting';
END
ELSE IF DB_ID(N'CF_Reporting') IS NOT NULL
    PRINT 'CF_Reporting already exists.';
ELSE
    PRINT 'ReportingDb not found; create CF_Reporting manually if needed.';
GO

SELECT name FROM sys.databases
WHERE name LIKE 'CF_%' OR name IN ('IdentityDb', 'FeedbackDb', 'ReportingDb')
ORDER BY name;
GO
