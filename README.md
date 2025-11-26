# 🚀 One Month Flow: C# Blazor & Web API

A project management system for tracking projects, teams, users, and their technology stacks.

## Features

- **Project Management**: Track projects with details, status, and repository URLs
- **Team Management**: Organize teams and assign them to projects
- **User Management**: Manage users and their GitHub accounts
- **Tech Stack Tracking**: Associate technology stacks with users, teams, and projects
- **Activity Logging**: Track project team activities
- **GitHub Integration**: Import repositories from GitHub organizations as projects

## Database

The application uses **Microsoft SQL Server** (MSSQL). See the [Scripts](Scripts/) directory for database setup scripts.

### Quick Setup

1. Create the database:
```sql
CREATE DATABASE OneMonthFlow;
GO
```

2. Run the complete setup script:
```sql
-- Execute Scripts/CompleteSetup.sql
```

This will create all tables, indexes, and populate sample data.

## GitHub Import

Import repositories from GitHub organizations (e.g., [one-project-one-month](https://github.com/one-project-one-month)) directly into your projects.

### Quick Start

```bash
cd OneMonthFlow.ConsoleApp
dotnet run -- GitHubImportProgram one-project-one-month
```

See [Scripts/ImportGitHubRepositories.md](Scripts/ImportGitHubRepositories.md) for detailed instructions.

## Configuration

### Connection String

Update `appsettings.json` with your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OneMonthFlow;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### GitHub Token (Optional)

For better GitHub API rate limits, add a token:

```json
{
  "GitHub": {
    "Organization": "one-project-one-month",
    "Token": "your-github-token"
  }
}
```

## Projects

- **OneMonthFlow.BlazorApp**: Blazor Server application (main UI)
- **OneMonthFlow.ConsoleApp**: Console application for GitHub imports
- **OneMonthFlow.Databases**: Database abstraction layer
- **OneMonthFlow.Domain**: Domain models and services

## Scripts

- `Scripts/CreateTables.sql`: Creates database schema
- `Scripts/InsertSampleData.sql`: Inserts sample data
- `Scripts/CompleteSetup.sql`: Complete database setup (tables + data)
- `Scripts/ImportGitHubRepositories.md`: GitHub import guide

## Technologies

- .NET 8.0
- Blazor Server
- Microsoft SQL Server
- Dapper (ORM)
- MudBlazor (UI components)

## License

[Add your license here]
