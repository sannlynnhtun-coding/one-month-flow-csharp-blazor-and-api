# GitHub Integration

This module provides functionality to import GitHub organization repositories into the OneMonthFlow system.

## Features

- Fetch all repositories from a GitHub organization
- Automatically create projects from repositories
- Map GitHub languages to tech stacks
- Associate tech stacks with projects based on repository language and topics
- Handle rate limiting and pagination

## Usage

### Console Application

Run the GitHub import tool using the console application:

```bash
dotnet run --project OneMonthFlow.ConsoleApp -- GitHubImportProgram one-project-one-month
```

Or set the organization in `appsettings.json`:

```json
{
  "GitHub": {
    "Organization": "one-project-one-month",
    "Token": "your-github-token-here"
  }
}
```

### GitHub Token (Optional but Recommended)

For unauthenticated requests, GitHub API allows 60 requests per hour. To increase this limit:

1. Create a GitHub Personal Access Token:
   - Go to GitHub Settings > Developer settings > Personal access tokens > Tokens (classic)
   - Generate a new token with `public_repo` scope

2. Set the token in one of these ways:
   - Add to `appsettings.json`: `"GitHub": { "Token": "your-token" }`
   - Set environment variable: `GITHUB_TOKEN=your-token`
   - Pass as command line argument (if implemented)

With authentication, you get 5,000 requests per hour.

## How It Works

1. **Fetch Repositories**: Uses GitHub API to fetch all repositories from the organization
2. **Create Projects**: Converts each repository into a project with:
   - Project Code: Generated from repository name (e.g., `pos_csharp` → `PROJ_POS_CSHARP`)
   - Project Name: Formatted repository name
   - Repository URL: GitHub repository URL
   - Description: Repository description
   - Status: Determined by repository activity (Active/In Progress/Completed/Archived)
   - Dates: Created and updated dates from repository

3. **Associate Tech Stacks**: Automatically maps repository language to tech stacks:
   - C# → C# .NET
   - JavaScript/TypeScript → JavaScript
   - Python → Python
   - Java → Java
   - React → React
   - Angular → Angular
   - SQL → SQL Server
   - Node.js → Node.js
   - And more...

4. **Skip Existing**: Automatically skips projects that already exist (based on project code)

## Models

### GitHubRepositoryModel
Represents a GitHub repository with all relevant fields from the GitHub API.

### ImportResult
Contains the results of the import operation:
- `TotalRepositories`: Total repositories found
- `Imported`: Successfully imported projects
- `Failed`: Failed imports
- `SkippedExisting`: Projects that already existed
- `SkippedArchived`: Archived repositories skipped
- `Errors`: List of error messages

## Example Output

```
=== GitHub Repository Import Tool ===

Organization: one-project-one-month
GitHub token: [CONFIGURED]

Do you want to proceed with importing repositories? (y/n): y

Starting import...

[Log messages showing progress...]

=== Import Results ===
Total repositories found: 104
Successfully imported: 98
Failed: 2
Skipped (existing): 4
Skipped (archived): 0

Import completed!
```

## Notes

- The import process includes delays to respect GitHub API rate limits
- Archived repositories are skipped by default
- Project codes are automatically generated and may need manual adjustment
- Tech stack associations are based on repository language and topics

