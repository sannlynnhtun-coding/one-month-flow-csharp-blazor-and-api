namespace OneMonthFlow.Domain.Features.Project;

public static class ProjectQueries
{
    public const string InsertProject = @"
            INSERT INTO Tbl_Project (ProjectId, ProjectCode, ProjectName, RepoUrl, StartDate, EndDate, ProjectDescription, Status)
            VALUES (@ProjectId, @ProjectCode, @ProjectName, @RepoUrl, @StartDate, @EndDate, @ProjectDescription, @Status);";

    public const string UpdateProject = @"
            UPDATE Tbl_Project SET
                ProjectCode = @ProjectCode,
                ProjectName = @ProjectName,
                RepoUrl = @RepoUrl,
                StartDate = @StartDate,
                EndDate = @EndDate,
                ProjectDescription = @ProjectDescription,
                Status = @Status
            WHERE ProjectId = @ProjectId;";

    public const string DeleteProject = "DELETE FROM Tbl_Project WHERE ProjectId = @ProjectId;";
    public const string GetProjectById = "SELECT * FROM Tbl_Project WHERE ProjectId = @ProjectId;";

    public const string InsertProjectTeam = @"
            INSERT INTO Tbl_ProjectTeam (ProjectTeamId, ProjectCode, TeamCode, ProjectTeamRating, Duration)
            VALUES (@ProjectTeamId, @ProjectCode, @TeamCode, @ProjectTeamRating, @Duration);";

    public const string DeleteProjectTeams = "DELETE FROM Tbl_ProjectTeam WHERE ProjectCode = @ProjectCode;";
    public const string GetProjectTeamsByProjectCode = "SELECT * FROM Tbl_ProjectTeam WHERE ProjectCode = @ProjectCode;";

    public const string GetTeamsByProjectId = @"
            SELECT T.* FROM Tbl_Team T
            INNER JOIN Tbl_ProjectTeam PT ON T.TeamCode = PT.TeamCode
            WHERE PT.ProjectCode = @ProjectCode;";

    // Example for GetProjectsPaginated - actual implementation might vary based on SQL dialect
    public static string GetProjectsPaginated(int page, int pageSize, string? filterColumn, string? filterValue)
    {
        // This is a simplified example. Real-world scenarios might need more complex dynamic SQL
        // or use a library that helps build dynamic queries safely.
        // Also, ensure filterColumn is validated to prevent SQL injection.
        var query = new StringBuilder("SELECT * FROM Tbl_Project ");
        if (!string.IsNullOrWhiteSpace(filterColumn) && !string.IsNullOrWhiteSpace(filterValue))
        {
            // VERY IMPORTANT: Sanitize/validate filterColumn to prevent SQL injection
            // For example, check against a list of allowed column names.
            var allowedColumns = new List<string> { "ProjectName", "ProjectCode", "Status" }; // Example
            if (allowedColumns.Contains(filterColumn, StringComparer.OrdinalIgnoreCase))
            {
                query.Append($"WHERE {filterColumn} LIKE @FilterValue ");
            }
        }
        query.Append("ORDER BY ProjectName "); // Default sort
        query.Append("OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");
        return query.ToString();
    }

    public static string GetProjectCount(string? filterColumn, string? filterValue)
    {
        var query = new StringBuilder("SELECT COUNT(*) FROM Tbl_Project ");
        if (!string.IsNullOrWhiteSpace(filterColumn) && !string.IsNullOrWhiteSpace(filterValue))
        {
            var allowedColumns = new List<string> { "ProjectName", "ProjectCode", "Status" };
            if (allowedColumns.Contains(filterColumn, StringComparer.OrdinalIgnoreCase))
            {
                query.Append($"WHERE {filterColumn} LIKE @FilterValue ");
            }
        }
        return query.ToString();
    }
}
