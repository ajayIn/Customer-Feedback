-- Use when old databases do not exist and you want empty CF_* catalogs.

IF DB_ID(N'CF_Identity') IS NULL
    CREATE DATABASE [CF_Identity];
IF DB_ID(N'CF_Feedback') IS NULL
    CREATE DATABASE [CF_Feedback];
IF DB_ID(N'CF_Reporting') IS NULL
    CREATE DATABASE [CF_Reporting];

SELECT name FROM sys.databases WHERE name IN (N'CF_Identity', N'CF_Feedback', N'CF_Reporting') ORDER BY name;
