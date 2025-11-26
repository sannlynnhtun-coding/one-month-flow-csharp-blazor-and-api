namespace OneMonthFlow.Domain.Features.UserTechStack;

public class UserTechStackService
{
    private readonly ISqlService _sqlService;

    public UserTechStackService(ISqlService sqlService) => _sqlService = sqlService;

    public async Task<Result<UserTechStackResponseModel>> AddUserTechStackAsync(UserTechStackRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.TechStackCode))
            {
                return Result<UserTechStackResponseModel>.ValidationError("UserCode and TechStackCode are required.");
            }

            var userTechStack = new UserTechStackModel
            {
                UserTechStackId = Guid.NewGuid().ToString(),
                UserCode = request.UserCode,
                TechStackCode = request.TechStackCode,
                ProficiencyLevel = request.ProficiencyLevel
            };

            int rowsAffected = await _sqlService.ExecuteAsync(UserTechStackQueries.InsertUserTechStack, userTechStack);
            var response = new UserTechStackResponseModel
            {
                UserTechStacks = new List<UserTechStackModel> { userTechStack },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };

            return rowsAffected > 0
                ? Result<UserTechStackResponseModel>.Success(response)
                : Result<UserTechStackResponseModel>.Failure("Failed to add tech stack to user.");
        }
        catch (Exception ex)
        {
            return Result<UserTechStackResponseModel>.Failure($"Error adding user tech stack: {ex.Message}");
        }
    }

    public async Task<Result<UserTechStackResponseModel>> GetUserTechStacksByUserAsync(string userCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<UserTechStackResponseModel>.ValidationError("UserCode is required.");
            }

            var techStacks = await _sqlService.QueryAsync<UserTechStackModel>(
                UserTechStackQueries.GetUserTechStacksByUserId, new { UserCode = userCode });

            var userTechStackModels = techStacks as UserTechStackModel[] ?? techStacks.ToArray();
            var response = new UserTechStackResponseModel
            {
                UserTechStacks = userTechStackModels.ToList(),
                TotalCount = userTechStackModels.Count(),
                Page = 1,
                PageSize = userTechStackModels.Count()
            };

            return Result<UserTechStackResponseModel>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<UserTechStackResponseModel>.Failure($"Error retrieving user tech stacks: {ex.Message}");
        }
    }

    public async Task<Result<UserTechStackResponseModel>> GetAllUserTechStacksAsync(int page = 1, int pageSize = 10, string? filterColumn = null, string? filterValue = null)
    {
        try
        {
            if (page < 1 || pageSize < 1)
            {
                return Result<UserTechStackResponseModel>.ValidationError("Page and PageSize must be greater than 0.");
            }

            var offset = (page - 1) * pageSize;
            var parameters = new
            {
                PageSize = pageSize,
                Offset = offset,
                FilterValue = $"%{filterValue}%"
            };

            var techStacks = await _sqlService.QueryAsync<UserTechStackModel>(
                UserTechStackQueries.GetUserTechStacksPaginated, parameters);

            var totalCount = await _sqlService.QuerySingleAsync<int>(
                UserTechStackQueries.GetUserTechStackCount, new { FilterValue = $"%{filterValue}%" });

            var response = new UserTechStackResponseModel
            {
                UserTechStacks = techStacks.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<UserTechStackResponseModel>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<UserTechStackResponseModel>.Failure($"Error retrieving all user tech stacks: {ex.Message}");
        }
    }

    public async Task<Result<UserTechStackResponseModel>> UpdateUserTechStackAsync(UserTechStackRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserTechStackId))
            {
                return Result<UserTechStackResponseModel>.ValidationError("UserTechStackId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.UserCode) || string.IsNullOrWhiteSpace(request.TechStackCode))
            {
                return Result<UserTechStackResponseModel>.ValidationError("UserCode and TechStackCode are required.");
            }

            var existing = await _sqlService.QuerySingleAsync<UserTechStackModel>(
                UserTechStackQueries.GetUserTechStackById, new { UserTechStackId = request.UserTechStackId });

            if (existing == null)
            {
                return Result<UserTechStackResponseModel>.NotFoundError($"UserTechStack with ID {request.UserTechStackId} not found.");
            }

            var userTechStackToUpdate = new UserTechStackModel
            {
                UserTechStackId = request.UserTechStackId,
                UserCode = request.UserCode,
                TechStackCode = request.TechStackCode,
                ProficiencyLevel = request.ProficiencyLevel
            };

            int rowsAffected = await _sqlService.ExecuteAsync(UserTechStackQueries.UpdateUserTechStack, userTechStackToUpdate);

            var response = new UserTechStackResponseModel
            {
                UserTechStacks = new List<UserTechStackModel> { userTechStackToUpdate },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };

            return rowsAffected > 0
                ? Result<UserTechStackResponseModel>.Success(response)
                : Result<UserTechStackResponseModel>.Failure("Failed to update user tech stack.");
        }
        catch (Exception ex)
        {
            return Result<UserTechStackResponseModel>.Failure($"Error updating user tech stack: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveUserTechStackAsync(string userTechStackId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userTechStackId))
            {
                return Result<bool>.ValidationError("UserTechStackId is required.");
            }

            int rowsAffected = await _sqlService.ExecuteAsync(UserTechStackQueries.DeleteUserTechStack, new { UserTechStackId = userTechStackId });

            return rowsAffected > 0
                ? Result<bool>.Success(true, "User tech stack removed successfully.")
                : Result<bool>.NotFoundError($"UserTechStack with ID {userTechStackId} not found.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error removing user tech stack: {ex.Message}");
        }
    }

    public async Task<Result<UserTechStackResponseModel>> SearchByGitHubAccountAsync(string githubAccountName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(githubAccountName))
            {
                return Result<UserTechStackResponseModel>.ValidationError("GitHubAccountName is required.");
            }

            var parameters = new { GitHubAccountSearch = $"%{githubAccountName}%" };

            var users = await _sqlService.QueryAsync<UserModel>(
                UserTechStackQueries.SearchUserTechStackByGitHubAccount, parameters);

            return Result<UserTechStackResponseModel>.Success(new UserTechStackResponseModel
            {
                Users = users.ToList()
            });
        }
        catch (Exception ex)
        {
            return Result<UserTechStackResponseModel>.Failure($"Error searching users by GitHub account: {ex.Message}");
        }
    }
}