namespace OneMonthFlow.Domain.Features.TechStack;

public static class TechStackQueries
{
    public const string InsertTechStack = @"
        INSERT INTO Tbl_TechStack (TechStackId, TechStackCode, TechStackShortCode, TechStackName)
        VALUES (@TechStackId, @TechStackCode, @TechStackShortCode, @TechStackName);";

    public const string GetTechStackById = @"
        SELECT * FROM Tbl_TechStack 
        WHERE TechStackId = @TechStackId;";

    public const string GetTechStacksPaginated = @"
        SELECT * FROM Tbl_TechStack
        WHERE TechStackName LIKE @FilterValue OR TechStackCode LIKE @FilterValue
        ORDER BY TechStackName
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

    public const string GetTechStackCount = @"
        SELECT COUNT(*) FROM Tbl_TechStack
        WHERE TechStackName LIKE @FilterValue OR TechStackCode LIKE @FilterValue;";

    public const string UpdateTechStack = @"
        UPDATE Tbl_TechStack 
        SET TechStackName = @TechStackName,
            TechStackCode = @TechStackCode,
            TechStackShortCode = @TechStackShortCode
        WHERE TechStackId = @TechStackId;";

    public const string DeleteTechStack = @"
        DELETE FROM Tbl_TechStack 
        WHERE TechStackId = @TechStackId;";

    public const string GetAllTechStacks = @"
            SELECT TechStackId, TechStackCode, TechStackShortCode, TechStackName 
            FROM Tbl_TechStack 
            ORDER BY TechStackName;";
}
