namespace OneMonthFlow.Domain.Features.ProjectTechStack;

public class ProjectTechStackPaginationRequestModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
}

public class ProjectTechStackPagedResponseModel
{
    public string ProjectCode { get; set; }
    public List<string> TechStackCodes { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

public static class ProjectTechStackQueries
{
    public const string InsertProjectTechStack = @"
        INSERT INTO Tbl_ProjectTechStack (ProjectTechStackId, ProjectCode, TechStackCode)
        VALUES (@ProjectTechStackId, @ProjectCode, @TechStackCode);
    ";

    public const string GetTechStacksByProjectCode = @"
        SELECT TechStackCode FROM Tbl_ProjectTechStack WHERE ProjectCode = @ProjectCode;
    ";

    public const string GetPagedTechStacksByProjectCode = @"
        WITH CTE AS (
            SELECT TechStackCode, ROW_NUMBER() OVER(ORDER BY TechStackCode) AS RowNum
            FROM Tbl_ProjectTechStack
            WHERE ProjectCode = @ProjectCode
        )
        SELECT TechStackCode
        FROM CTE
        WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize;
    ";

    public const string CountTechStacksByProjectCode = @"
        SELECT COUNT(*) FROM Tbl_ProjectTechStack WHERE ProjectCode = @ProjectCode;
    ";

    public const string DeleteByProjectCode = @"
        DELETE FROM Tbl_ProjectTechStack WHERE ProjectCode = @ProjectCode;
    ";

    public const string DeleteByProjectAndTechStack = @"
        DELETE FROM Tbl_ProjectTechStack 
        WHERE ProjectCode = @ProjectCode AND TechStackCode = @TechStackCode;
    ";

    public const string UpdateTechStack = @"
        UPDATE Tbl_ProjectTechStack
        SET TechStackCode = @NewTechStackCode
        WHERE ProjectCode = @ProjectCode AND TechStackCode = @OldTechStackCode;
    ";

    public const string GetAllTechStacks = @"
        SELECT * FROM Tbl_TechStack
        ORDER BY TechStackName;"; 
}

public class ProjectTechStackResponseModel
{
    public string ProjectCode { get; set; }
    public List<string> TechStackCodes { get; set; }
}

public class ProjectTechStackRequestModel
{
    public string ProjectCode { get; set; }
    public List<string> TechStackCodes { get; set; } = new List<string>();
}

public class UpdateProjectTechStackRequestModel
{
    public string ProjectCode { get; set; }
    public string OldTechStackCode { get; set; }
    public string NewTechStackCode { get; set; }
}

public class DeleteProjectTechStackRequestModel
{
    public string ProjectCode { get; set; }
    public string TechStackCode { get; set; }
}

public class ProjectTechStackService
{
    private readonly ISqlService _sqlService;

    public ProjectTechStackService(ISqlService sqlService) => _sqlService = sqlService;

    public async Task<Result<ProjectTechStackResponseModel>> CreateProjectTechStacksAsync(ProjectTechStackRequestModel request)
    {
        try
        {
            if (request == null)
                return Result<ProjectTechStackResponseModel>.ValidationError("Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.ProjectCode))
                return Result<ProjectTechStackResponseModel>.ValidationError("ProjectCode is required.");

            if (request.TechStackCodes == null || !request.TechStackCodes.Any())
                return Result<ProjectTechStackResponseModel>.ValidationError("At least one TechStackCode is required.");

            var validTechStacks = request.TechStackCodes
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Distinct()
                .ToList();

            if (!validTechStacks.Any())
                return Result<ProjectTechStackResponseModel>.ValidationError("No valid TechStackCodes provided.");

            var existing = await _sqlService.QueryAsync<string>(
                ProjectTechStackQueries.GetTechStacksByProjectCode,
                new { request.ProjectCode });

            var existingSet = existing.ToHashSet();

            int totalInserted = 0;

            foreach (var techStackCode in validTechStacks)
            {
                if (existingSet.Contains(techStackCode)) continue;

                var item = new
                {
                    ProjectTechStackId = Guid.NewGuid().ToString(),
                    request.ProjectCode,
                    TechStackCode = techStackCode
                };

                totalInserted += await _sqlService.ExecuteAsync(
                    ProjectTechStackQueries.InsertProjectTechStack,
                    item);
            }

            if (totalInserted == 0)
                return Result<ProjectTechStackResponseModel>.Failure("All provided tech stacks already exist.");

            var response = new ProjectTechStackResponseModel
            {
                ProjectCode = request.ProjectCode,
                TechStackCodes = validTechStacks
            };

            return Result<ProjectTechStackResponseModel>.Success(response, $"{totalInserted} tech stack(s) assigned.");
        }
        catch (Exception ex)
        {
            return Result<ProjectTechStackResponseModel>.Failure($"Error assigning tech stacks: {ex.Message}");
        }
    }

    public async Task<Result<ProjectTechStackPagedResponseModel>> GetPagedTechStacksByProjectCodeAsync(string projectCode, ProjectTechStackPaginationRequestModel pagination)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Result<ProjectTechStackPagedResponseModel>.ValidationError("ProjectCode is required.");

            var totalCount = await _sqlService.QuerySingleAsync<int>(
                ProjectTechStackQueries.CountTechStacksByProjectCode,
                new { ProjectCode = projectCode });

            if (totalCount == 0)
                return Result<ProjectTechStackPagedResponseModel>.Success(new ProjectTechStackPagedResponseModel
                {
                    ProjectCode = projectCode,
                    TechStackCodes = new List<string>(),
                    TotalCount = 0,
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize
                }, "No tech stacks found.");

            var techStacks = await _sqlService.QueryAsync<string>(
                ProjectTechStackQueries.GetPagedTechStacksByProjectCode,
                new
                {
                    ProjectCode = projectCode,
                    pagination.PageNumber,
                    pagination.PageSize
                });

            var response = new ProjectTechStackPagedResponseModel
            {
                ProjectCode = projectCode,
                TechStackCodes = techStacks.ToList(),
                TotalCount = totalCount,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize
            };

            return Result<ProjectTechStackPagedResponseModel>.Success(response, "Retrieved paged tech stacks successfully.");
        }
        catch (Exception ex)
        {
            return Result<ProjectTechStackPagedResponseModel>.Failure($"Error retrieving tech stacks: {ex.Message}");
        }
    }

    public async Task<Result<ProjectTechStackResponseModel>> UpdateTechStackAsync(UpdateProjectTechStackRequestModel request)
    {
        try
        {
            if (request == null)
                return Result<ProjectTechStackResponseModel>.ValidationError("Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.ProjectCode) ||
                string.IsNullOrWhiteSpace(request.OldTechStackCode) ||
                string.IsNullOrWhiteSpace(request.NewTechStackCode))
            {
                return Result<ProjectTechStackResponseModel>.ValidationError("All fields are required.");
            }

            int rowsAffected = await _sqlService.ExecuteAsync(
                ProjectTechStackQueries.UpdateTechStack,
                new
                {
                    ProjectCode = request.ProjectCode,
                    OldTechStackCode = request.OldTechStackCode,
                    NewTechStackCode = request.NewTechStackCode
                });

            if (rowsAffected == 0)
            {
                return Result<ProjectTechStackResponseModel>.Failure("Failed to update tech stack.");
            }

            var updatedTechStacks = await _sqlService.QueryAsync<string>(
                ProjectTechStackQueries.GetTechStacksByProjectCode,
                new { ProjectCode = request.ProjectCode });

            var response = new ProjectTechStackResponseModel
            {
                ProjectCode = request.ProjectCode,
                TechStackCodes = updatedTechStacks.ToList()
            };

            return Result<ProjectTechStackResponseModel>.Success(response, "Tech stack updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<ProjectTechStackResponseModel>.Failure($"Error updating tech stack: {ex.Message}");
        }
    }

    public async Task<Result<ProjectTechStackResponseModel>> DeleteTechStacksByProjectCodeAsync(string projectCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectCode))
            {
                return Result<ProjectTechStackResponseModel>.ValidationError("ProjectCode is required.");
            }

            int rowsAffected = await _sqlService.ExecuteAsync(
                ProjectTechStackQueries.DeleteByProjectCode,
                new { ProjectCode = projectCode });

            if (rowsAffected == 0)
            {
                return Result<ProjectTechStackResponseModel>.Failure("No tech stacks found for deletion.");
            }

            // Return empty list as all were deleted
            var response = new ProjectTechStackResponseModel
            {
                ProjectCode = projectCode,
                TechStackCodes = new List<string>()
            };

            return Result<ProjectTechStackResponseModel>.Success(response, "All tech stacks deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<ProjectTechStackResponseModel>.Failure($"Error deleting tech stacks: {ex.Message}");
        }
    }

    public async Task<Result<ProjectTechStackResponseModel>> DeleteTechStackByProjectAndCodeAsync(DeleteProjectTechStackRequestModel request)
    {
        try
        {
            if (request == null)
                return Result<ProjectTechStackResponseModel>.ValidationError("Request cannot be null.");

            if (string.IsNullOrWhiteSpace(request.ProjectCode) || string.IsNullOrWhiteSpace(request.TechStackCode))
            {
                return Result<ProjectTechStackResponseModel>.ValidationError("Both ProjectCode and TechStackCode are required.");
            }

            int rowsAffected = await _sqlService.ExecuteAsync(
                ProjectTechStackQueries.DeleteByProjectAndTechStack,
                new { ProjectCode = request.ProjectCode, TechStackCode = request.TechStackCode });

            if (rowsAffected == 0)
            {
                return Result<ProjectTechStackResponseModel>.Failure("Tech stack not found or could not be deleted.");
            }

            var updatedTechStacks = await _sqlService.QueryAsync<string>(
                ProjectTechStackQueries.GetTechStacksByProjectCode,
                new { ProjectCode = request.ProjectCode });

            var response = new ProjectTechStackResponseModel
            {
                ProjectCode = request.ProjectCode,
                TechStackCodes = updatedTechStacks.ToList()
            };

            return Result<ProjectTechStackResponseModel>.Success(response, "Tech stack deleted successfully.");
        }
        catch (Exception ex)
        {
            return Result<ProjectTechStackResponseModel>.Failure($"Error deleting tech stack: {ex.Message}");
        }
    }
}