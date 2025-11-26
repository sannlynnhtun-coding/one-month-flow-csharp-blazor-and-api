# Import GitHub Repositories

This guide explains how to import repositories from the GitHub organization `one-project-one-month` into your OneMonthFlow database.

## Prerequisites

1. Database must be set up (run `CompleteSetup.sql` first)
2. Connection string configured in `appsettings.json`
3. (Optional) GitHub Personal Access Token for higher rate limits

## Steps

### 1. Configure Connection String

Ensure your `appsettings.json` has the correct database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OneMonthFlow;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### 2. (Optional) Configure GitHub Token

For better rate limits, add your GitHub token:

```json
{
  "GitHub": {
    "Organization": "one-project-one-month",
    "Token": "ghp_your_token_here"
  }
}
```

Or set environment variable:
```bash
export GITHUB_TOKEN=ghp_your_token_here
```

### 3. Run the Import Tool

Navigate to the project root and run:

```bash
cd OneMonthFlow.ConsoleApp
dotnet run -- GitHubImportProgram one-project-one-month
```

Or if you've configured the organization in `appsettings.json`:

```bash
dotnet run --project OneMonthFlow.ConsoleApp -- GitHubImportProgram
```

### 4. Review Results

The tool will display:
- Total repositories found
- Successfully imported projects
- Failed imports
- Skipped projects (existing or archived)
- Any errors encountered

## What Gets Imported

For each repository, the following data is imported:

- **Project Code**: Auto-generated from repository name
- **Project Name**: Formatted repository name
- **Repository URL**: GitHub repository URL
- **Description**: Repository description
- **Start Date**: Repository creation date
- **End Date**: Last update date
- **Status**: Based on activity (Active/In Progress/Completed)
- **Tech Stacks**: Automatically associated based on repository language

## Troubleshooting

### Rate Limit Errors

If you see rate limit errors:
- Wait for the rate limit to reset (usually 1 hour)
- Use a GitHub Personal Access Token for higher limits (5,000/hour vs 60/hour)

### Connection Errors

- Verify your database connection string
- Ensure SQL Server is running
- Check firewall settings

### Import Failures

- Check the error messages in the output
- Verify project codes don't exceed 50 characters
- Ensure required fields are present in the database

## After Import

After importing, you can:
1. View projects in the Blazor application
2. Manually adjust project details if needed
3. Associate teams with projects
4. Add project activities

