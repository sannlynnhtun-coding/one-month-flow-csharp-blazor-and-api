namespace OneMonthFlow.Domain.Features.TeamUser;

public static class TeamUserQueries
{
    public const string InsertTeamUser = @"
            INSERT INTO Tbl_TeamUser (TeamUserId, TeamCode, UserCode, UserRating)
            VALUES (@TeamUserId, @TeamCode, @UserCode, @UserRating);";

    public const string GetTeamUserModelsWithDetailsByTeamCode = @"
            SELECT tu.TeamUserId, tu.TeamCode, tu.UserCode, tu.UserRating, u.UserName, u.GitHubAccountName, u.Nrc
            FROM Tbl_TeamUser tu
            INNER JOIN Tbl_User u ON tu.UserCode = u.UserCode
            WHERE tu.TeamCode = @TeamCode;";

    public const string GetUserModelsByTeamCode = @"
            SELECT u.UserId, u.UserCode, u.UserName, u.GitHubAccountName, u.Nrc, u.MobileNo
            FROM Tbl_User u
            INNER JOIN Tbl_TeamUser tu ON u.UserCode = tu.UserCode
            WHERE tu.TeamCode = @TeamCode;";

    public const string GetTeamUsersPaginated = @"
            SELECT tu.TeamUserId, tu.TeamCode, tu.UserCode, tu.UserRating, t.TeamName, u.UserName
            FROM Tbl_TeamUser tu
            INNER JOIN Tbl_Team t ON tu.TeamCode = t.TeamCode 
            INNER JOIN Tbl_User u ON tu.UserCode = u.UserCode 
            WHERE (@FilterValue = '%' OR t.TeamName LIKE @FilterValue OR u.UserName LIKE @FilterValue OR tu.TeamCode LIKE @FilterValue OR tu.UserCode LIKE @FilterValue)
            ORDER BY t.TeamName, u.UserName
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

    public const string GetTeamUserCount = @"
            SELECT COUNT(DISTINCT tu.TeamUserId) 
            FROM Tbl_TeamUser tu
            INNER JOIN Tbl_Team t ON tu.TeamCode = t.TeamCode
            INNER JOIN Tbl_User u ON tu.UserCode = u.UserCode
            WHERE (@FilterValue = '%' OR t.TeamName LIKE @FilterValue OR u.UserName LIKE @FilterValue OR tu.TeamCode LIKE @FilterValue OR tu.UserCode LIKE @FilterValue);";

    public const string DeleteTeamUser = "DELETE FROM Tbl_TeamUser WHERE TeamUserId = @TeamUserId;";

    public const string DeleteTeamUserByTeamAndUserCode = @"
            DELETE FROM Tbl_TeamUser 
            WHERE TeamCode = @TeamCode AND UserCode = @UserCode;";

    public const string GetTeamByCode = @"
            SELECT TeamId, TeamCode, TeamName
            FROM Tbl_Team 
            WHERE TeamCode = @TeamCode;";

    public const string GetTeamUserByTeamAndUser = @"
            SELECT tu.TeamUserId, tu.TeamCode, tu.UserCode, tu.UserRating
            FROM Tbl_TeamUser tu
            WHERE tu.TeamCode = @TeamCode AND tu.UserCode = @UserCode;";

    public const string CheckTeamExists = @"
            SELECT COUNT(1) 
            FROM Tbl_Team 
            WHERE TeamCode = @TeamCode;";

    public const string CheckUserExists = @"
            SELECT COUNT(1) 
            FROM Tbl_User 
            WHERE UserCode = @UserCode;";

    public const string SearchUsers = @"
            SELECT UserId, UserCode, UserName, GitHubAccountName, Nrc, MobileNo
            FROM Tbl_User
            WHERE (@Query = '%' OR UserName LIKE @Query OR UserCode LIKE @Query OR GitHubAccountName LIKE @Query OR MobileNo LIKE @Query);";

    public const string SearchTeams = @"
            SELECT TeamId, TeamCode, TeamName
            FROM Tbl_Team
            WHERE (@Query = '%' OR TeamCode LIKE @Query OR TeamName LIKE @Query);";
}
