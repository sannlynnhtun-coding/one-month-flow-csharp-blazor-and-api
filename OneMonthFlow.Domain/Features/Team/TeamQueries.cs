namespace OneMonthFlow.Domain.Features.Team;

public static class TeamQueries
{
    public const string InsertTeam = @"
        INSERT INTO Tbl_Team (TeamId, TeamCode, TeamName)
        VALUES (@TeamId, @TeamCode, @TeamName);";

    public const string GetTeamById = "SELECT * FROM Tbl_Team WHERE TeamId = @TeamId;";

    public const string GetTeamByCode = "SELECT * FROM Tbl_Team WHERE TeamCode = @TeamCode;";

    public const string GetTeamsPaginated = @"
        SELECT * FROM Tbl_Team
        WHERE (@FilterValue = '%' OR TeamName LIKE @FilterValue OR TeamCode LIKE @FilterValue)
        ORDER BY TeamName
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

    public const string GetTeamCount = @"
        SELECT COUNT(*) FROM Tbl_Team
        WHERE (@FilterValue = '%' OR TeamName LIKE @FilterValue OR TeamCode LIKE @FilterValue);";

    public const string UpdateTeam = @"
        UPDATE Tbl_Team SET TeamName = @TeamName WHERE TeamId = @TeamId;";

    public const string DeleteTeam = "DELETE FROM Tbl_Team WHERE TeamId = @TeamId;";

    public const string InsertTeamTechStack = @"
        INSERT INTO Tbl_TeamTechStack (TeamTechStackId, TeamCode, TechStackCode)
        VALUES (@TeamTechStackId, @TeamCode, @TechStackCode);";

    public const string DeleteTeamTechStacksByTeamCode = @"
        DELETE FROM Tbl_TeamTechStack WHERE TeamCode = @TeamCode;";

    public const string GetTechStackCodesByTeamCode = @"
        SELECT TechStackCode FROM Tbl_TeamTechStack WHERE TeamCode = @TeamCode;";
}
