namespace OneMonthFlow.Domain.Features.User;

public static class UserQueries
{
    public const string InsertUser = @"
        INSERT INTO Tbl_User (UserId, UserCode, UserName, GitHubAccountName, Nrc, MobileNo)
        VALUES (@UserId, @UserCode, @UserName, @GitHubAccountName, @Nrc, @MobileNo);";

    public const string GetUserById = @"
        SELECT UserId, UserCode, UserName, GitHubAccountName, Nrc, MobileNo FROM Tbl_User 
        WHERE UserId = @UserId;";

    public const string GetUserByUserCode = @"
        SELECT UserId, UserCode, UserName, GitHubAccountName, Nrc, MobileNo FROM Tbl_User 
        WHERE UserCode = @UserCode;";

    public const string GetUsersPaginated = @"
        SELECT UserId, UserCode, UserName, GitHubAccountName, Nrc, MobileNo
        FROM Tbl_User
        WHERE (@FilterValue IS NULL OR UserName LIKE @FilterValue OR UserCode LIKE @FilterValue OR GitHubAccountName LIKE @FilterValue)
        ORDER BY UserName
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

    public const string GetUserCount = @"
        SELECT COUNT(*) FROM Tbl_User
        WHERE (@FilterValue IS NULL OR UserName LIKE @FilterValue OR UserCode LIKE @FilterValue OR GitHubAccountName LIKE @FilterValue);";

    public const string UpdateUser = @"
        UPDATE Tbl_User
        SET UserName = @UserName,
            GitHubAccountName = @GitHubAccountName,
            Nrc = @Nrc,
            MobileNo = @MobileNo
        WHERE UserId = @UserId;";

    public const string DeleteUser = "DELETE FROM Tbl_User WHERE UserId = @UserId;";

    public const string InsertUserTechStack = @"
        INSERT INTO Tbl_UserTechStack (UserTechStackId, UserCode, TechStackCode, ProficiencyLevel)
        VALUES (@UserTechStackId, @UserCode, @TechStackCode, @ProficiencyLevel);";

    public const string GetUserTechStacksByUserCode = @"
        SELECT ts.TechStackId, uts.TechStackCode, ts.TechStackName, ts.TechStackShortCode, uts.ProficiencyLevel
        FROM Tbl_UserTechStack uts
        JOIN Tbl_TechStack ts ON uts.TechStackCode = ts.TechStackCode
        WHERE uts.UserCode = @UserCode;";

    public const string DeleteUserTechStacksByUserCode = @"
        DELETE FROM Tbl_UserTechStack WHERE UserCode = @UserCode;";
}