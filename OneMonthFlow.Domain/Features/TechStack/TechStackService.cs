namespace OneMonthFlow.Domain.Features.TechStack;

public class TechStackService
{
    private readonly ISqlService _sqlService;

    public TechStackService(ISqlService sqlService) => _sqlService = sqlService;

    public async Task<Result<TechStackResponseModel>> CreateTechStackAsync(TechStackRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.TechStackCode) || string.IsNullOrWhiteSpace(request.TechStackName))
                return Result<TechStackResponseModel>.ValidationError("TechStackCode and TechStackName are required.");

            var techStack = new TechStackModel
            {
                TechStackId = Guid.NewGuid().ToString(),
                TechStackCode = request.TechStackCode,
                TechStackShortCode = request.TechStackShortCode,
                TechStackName = request.TechStackName
            };

            int rowsAffected = await _sqlService.ExecuteAsync(TechStackQueries.InsertTechStack, techStack);

            if (rowsAffected == 0)
                return Result<TechStackResponseModel>.Failure("Failed to create tech stack.");

            return Result<TechStackResponseModel>.Success(new TechStackResponseModel
            {
                TechStacks = new List<TechStackModel> { techStack },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            });
        }
        catch (Exception ex)
        {
            return Result<TechStackResponseModel>.Failure($"Error creating tech stack: {ex.Message}");
        }
    }

    public async Task<Result<TechStackResponseModel>> GetTechStackByIdAsync(string techStackId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(techStackId))
                return Result<TechStackResponseModel>.ValidationError("TechStackId is required.");

            var techStack = await _sqlService.QuerySingleAsync<TechStackModel>(
                TechStackQueries.GetTechStackById, new { TechStackId = techStackId });

            if (techStack == null)
                return Result<TechStackResponseModel>.NotFoundError($"TechStack with ID {techStackId} not found.");

            return Result<TechStackResponseModel>.Success(new TechStackResponseModel
            {
                TechStacks = new List<TechStackModel> { techStack },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            });
        }
        catch (Exception ex)
        {
            return Result<TechStackResponseModel>.Failure($"Error retrieving tech stack: {ex.Message}");
        }
    }

    public async Task<Result<TechStackResponseModel>> GetTechStacksAsync(int page = 1, int pageSize = 10, string filterValue = "")
    {
        try
        {
            if (page < 1 || pageSize < 1)
                return Result<TechStackResponseModel>.ValidationError("Page and PageSize must be greater than 0.");

            var offset = (page - 1) * pageSize;
            var filter = string.IsNullOrEmpty(filterValue) ? "%" : $"%{filterValue}%";

            var parameters = new
            {
                PageSize = pageSize,
                Offset = offset,
                FilterValue = filter
            };

            var techStacks = await _sqlService.QueryAsync<TechStackModel>(
                TechStackQueries.GetTechStacksPaginated, parameters);

            var totalCount = await _sqlService.QuerySingleAsync<int>(
                TechStackQueries.GetTechStackCount, new { FilterValue = filter });

            return Result<TechStackResponseModel>.Success(new TechStackResponseModel
            {
                TechStacks = techStacks.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            return Result<TechStackResponseModel>.Failure($"Error retrieving tech stacks: {ex.Message}");
        }
    }

    public async Task<Result<TechStackResponseModel>> UpdateTechStackAsync(string techStackId, TechStackRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(techStackId))
                return Result<TechStackResponseModel>.ValidationError("TechStackId is required.");

            if (string.IsNullOrWhiteSpace(request.TechStackCode) || string.IsNullOrWhiteSpace(request.TechStackName))
                return Result<TechStackResponseModel>.ValidationError("TechStackCode and TechStackName are required.");

            var techStack = new TechStackModel
            {
                TechStackId = techStackId,
                TechStackCode = request.TechStackCode,
                TechStackShortCode = request.TechStackShortCode,
                TechStackName = request.TechStackName
            };

            int rowsAffected = await _sqlService.ExecuteAsync(TechStackQueries.UpdateTechStack, techStack);

            if (rowsAffected == 0)
                return Result<TechStackResponseModel>.NotFoundError($"TechStack with ID {techStackId} not found.");

            return Result<TechStackResponseModel>.Success(new TechStackResponseModel
            {
                TechStacks = new List<TechStackModel> { techStack },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            });
        }
        catch (Exception ex)
        {
            return Result<TechStackResponseModel>.Failure($"Error updating tech stack: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteTechStackAsync(string techStackId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(techStackId))
                return Result<bool>.ValidationError("TechStackId is required.");

            int rowsAffected = await _sqlService.ExecuteAsync(
                TechStackQueries.DeleteTechStack, new { TechStackId = techStackId });

            return rowsAffected > 0
                ? Result<bool>.Success(true, "TechStack deleted successfully.")
                : Result<bool>.NotFoundError($"TechStack with ID {techStackId} not found.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting tech stack: {ex.Message}");
        }
    }

    public async Task<Result<List<TechStackModel>>> GetAllTechStacksAsync()
    {
        try
        {
            var techStacks = await _sqlService.QueryAsync<TechStackModel>(TechStackQueries.GetAllTechStacks);
            return Result<List<TechStackModel>>.Success(techStacks.ToList(), "All tech stacks retrieved successfully.");
        }
        catch (Exception ex)
        {
            return Result<List<TechStackModel>>.Failure($"Error retrieving all tech stacks: {ex.Message}");
        }
    }
}
