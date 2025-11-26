# OneMonthFlow Database Migration Scripts

This directory contains MSSQL Server scripts to create the database schema and populate it with sample data.

## Prerequisites

- SQL Server 2019 or later
- Database named `OneMonthFlow` (create it if it doesn't exist)

## Scripts

### 1. CreateTables.sql
Creates all database tables, indexes, and foreign key constraints.

**Usage:**
```sql
-- First, create the database if it doesn't exist
CREATE DATABASE OneMonthFlow;
GO

-- Then run the script
USE [OneMonthFlow];
GO
-- Execute CreateTables.sql
```

### 2. InsertSampleData.sql
Populates the database with sample data for testing and development.

**Usage:**
```sql
USE [OneMonthFlow];
GO
-- Execute InsertSampleData.sql
```

## Database Schema

The database consists of the following tables:

- **Tbl_User** - User information
- **Tbl_Team** - Team information
- **Tbl_TechStack** - Technology stack definitions
- **Tbl_Project** - Project information
- **Tbl_UserTechStack** - User technology skills
- **Tbl_TeamUser** - Team membership
- **Tbl_TeamTechStack** - Team technology requirements
- **Tbl_ProjectTeam** - Project team assignments
- **Tbl_ProjectTechStack** - Project technology requirements
- **Tbl_ProjectTeamActivity** - Project activity logs

## Connection String

Update your `appsettings.json` with the appropriate connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OneMonthFlow;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"
  }
}
```

## Notes

- All ID fields use `NVARCHAR(50)` to store GUIDs as strings
- Foreign key relationships are enforced
- Indexes are created on frequently queried columns
- Sample data includes 5 users, 4 teams, 8 tech stacks, and 4 projects

