namespace OneMonthFlow.Domain.Features.UserTechStack;

public static class UserTechStackQueries
{
    public const string InsertUserTechStack = @"
        INSERT INTO Tbl_UserTechStack (UserTechStackId, UserCode, TechStackCode, ProficiencyLevel)
        VALUES (@UserTechStackId, @UserCode, @TechStackCode, @ProficiencyLevel);";

    public const string GetUserTechStacksByUserCode = @"
        SELECT uts.*, t.TechStackName
        FROM Tbl_UserTechStack uts
        JOIN Tbl_TechStack t ON t.TechStackCode = uts.TechStackCode
        WHERE uts.UserCode = @UserCode;";

    public const string GetUserTechStacksByUserId = @"
        SELECT uts.*, t.TechStackName
        FROM Tbl_UserTechStack uts
        JOIN Tbl_TechStack t ON t.TechStackCode = uts.TechStackCode
        JOIN Tbl_User u ON u.UserCode = uts.UserCode
        WHERE u.UserId = @UserId;";

    public const string GetUserTechStacksPaginated = @"
        SELECT uts.UserTechStackId,
               uts.UserCode,
               u.UserName,
               uts.TechStackCode,
               t.TechStackName,
               uts.ProficiencyLevel
        FROM Tbl_UserTechStack uts
        JOIN Tbl_User u ON u.UserCode = uts.UserCode
        JOIN Tbl_TechStack t ON t.TechStackCode = uts.TechStackCode
        WHERE (@FilterValue IS NULL OR u.UserName LIKE @FilterValue OR u.UserCode LIKE @FilterValue OR t.TechStackName LIKE @FilterValue OR t.TechStackCode LIKE @FilterValue)
        ORDER BY u.UserName
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

    public const string GetUserTechStackCount = @"
        SELECT COUNT(*)
        FROM Tbl_UserTechStack uts
        JOIN Tbl_User u ON u.UserCode = uts.UserCode
        JOIN Tbl_TechStack t ON t.TechStackCode = uts.TechStackCode
        WHERE (@FilterValue IS NULL OR u.UserName LIKE @FilterValue OR u.UserCode LIKE @FilterValue OR t.TechStackName LIKE @FilterValue OR t.TechStackCode LIKE @FilterValue);";

    public const string GetUserTechStackById = @"
    SELECT uts.*, u.UserCode, u.UserName, t.TechStackCode, t.TechStackName
    FROM Tbl_UserTechStack uts
    JOIN Tbl_User u ON u.UserCode = uts.UserCode
    JOIN Tbl_TechStack t ON t.TechStackCode = uts.TechStackCode
    WHERE uts.UserTechStackId = @UserTechStackId;";

    public const string UpdateUserTechStack = @"
    UPDATE Tbl_UserTechStack
    SET 
        UserCode = @UserCode,
        TechStackCode = @TechStackCode,
        ProficiencyLevel = @ProficiencyLevel,
        ModifiedDate = GETDATE(),
        ModifiedBy = @ModifiedBy
    WHERE UserTechStackId = @UserTechStackId;";

    public const string DeleteUserTechStack = @"
        DELETE FROM Tbl_UserTechStack
        WHERE UserTechStackId = @UserTechStackId;";

    public const string SearchUserTechStackByGitHubAccount = @"
    SELECT 
        UserCode,
        UserName,
        GitHubAccountName
    FROM Tbl_User
    WHERE GitHubAccountName LIKE @GitHubAccountSearch
    ORDER BY UserName;";
}