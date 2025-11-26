namespace OneMonthFlow.Domain.Features.User;

public class UserService
{
    private readonly ISqlService _sqlService;

    public UserService(ISqlService sqlService) => _sqlService = sqlService;

    private string GenerateUserCode() => Ulid.NewUlid().ToString();
    private string GenerateUserTechStackId() => Guid.NewGuid().ToString();

    public async Task<Result<UserResponseModel>> CreateUserAsync(UserRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Result<UserResponseModel>.ValidationError("UserName is required.");
        }

        var user = new UserModel
        {
            UserId = Guid.NewGuid().ToString(),
            UserCode = GenerateUserCode(),
            UserName = request.UserName,
            GitHubAccountName = request.GitHubAccountName,
            Nrc = request.Nrc,
            MobileNo = request.MobileNo
            // CreatedDate = DateTime.UtcNow; // If you add this field
        };

        // IDbTransaction transaction = null; // Example for Dapper
        try
        {
            // transaction = await _sqlService.BeginTransactionAsync(); // Or however your service handles it

            int rowsAffected = await _sqlService.ExecuteAsync(UserQueries.InsertUser, user /*, transaction*/);
            if (rowsAffected == 0)
            {
                // await _sqlService.RollbackTransactionAsync(transaction);
                return Result<UserResponseModel>.Failure("Failed to create user.");
            }

            if (request.TechStackCodes != null && request.TechStackCodes.Any())
            {
                foreach (var techStackCode in request.TechStackCodes)
                {
                    var userTechStack = new
                    {
                        UserTechStackId = GenerateUserTechStackId(),
                        UserCode = user.UserCode,
                        TechStackCode = techStackCode,
                        ProficiencyLevel = (int?)null // Or a default value if applicable
                    };
                    await _sqlService.ExecuteAsync(UserQueries.InsertUserTechStack, userTechStack /*, transaction*/);
                }
            }

            // await _sqlService.CommitTransactionAsync(transaction);

            // Fetch created user with tech stacks for response
            var createdUserWithStacks = await GetUserWithTechStacks(user.UserId);
            if (!createdUserWithStacks.IsSuccess) return Result<UserResponseModel>.Failure("User created, but failed to retrieve details.");

            var response = new UserResponseModel
            {
                Users = new List<UserWithTechStacksModel> { new() { User = createdUserWithStacks.Data } },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };
            return Result<UserResponseModel>.Success(response);
        }
        catch (Exception ex)
        {
            // await _sqlService.RollbackTransactionAsync(transaction);
            return Result<UserResponseModel>.Failure($"Error creating user: {ex.Message}");
        }
    }

    public async Task<Result<UserResponseModel>> GetUserByIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<UserResponseModel>.ValidationError("UserId is required.");
        }

        try
        {
            var userWithStacks = await GetUserWithTechStacks(userId);
            if (!userWithStacks.IsSuccess || userWithStacks.Data == null)
            {
                return Result<UserResponseModel>.NotFoundError($"User with ID {userId} not found.");
            }

            var response = new UserResponseModel
            {
                Users = new List<UserWithTechStacksModel> { new() { User = userWithStacks.Data } },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };
            return Result<UserResponseModel>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<UserResponseModel>.Failure($"Error retrieving user: {ex.Message}");
        }
    }

    private async Task<Result<UserModel>> GetUserWithTechStacks(string userIdOrCode, bool isUserCode = false)
    {
        UserModel user;
        if (isUserCode)
        {
            user = await _sqlService.QuerySingleAsync<UserModel>(UserQueries.GetUserByUserCode, new { UserCode = userIdOrCode });
        }
        else
        {
            user = await _sqlService.QuerySingleAsync<UserModel>(UserQueries.GetUserById, new { UserId = userIdOrCode });
        }

        if (user == null)
        {
            return Result<UserModel>.NotFoundError("User not found.");
        }

        var techStacks = await _sqlService.QueryAsync<TechStackModel>(UserQueries.GetUserTechStacksByUserCode, new { UserCode = user.UserCode });
        user.TechStacks = techStacks.ToList();

        return Result<UserModel>.Success(user);
    }


    public async Task<Result<UserListResponseModel>> GetUsersAsync(int page = 1, int pageSize = 10, string filterColumn = null, string filterValue = null)
    {
        if (page < 1 || pageSize < 1)
        {
            return Result<UserListResponseModel>.ValidationError("Page and PageSize must be greater than 0.");
        }

        try
        {
            var offset = (page - 1) * pageSize;
            var filterParam = string.IsNullOrWhiteSpace(filterValue) ? null : $"%{filterValue}%";
            var parameters = new { PageSize = pageSize, Offset = offset, FilterValue = filterParam };

            var usersData = await _sqlService.QueryAsync<UserModel>(UserQueries.GetUsersPaginated, parameters);
            var users = usersData.ToList();
            var totalCount = await _sqlService.QuerySingleAsync<int>(UserQueries.GetUserCount, new { FilterValue = filterParam });

            foreach (var user in users)
            {
                var techStacks = await _sqlService.QueryAsync<TechStackModel>(UserQueries.GetUserTechStacksByUserCode, new { UserCode = user.UserCode });
                user.TechStacks = techStacks.ToList();
            }

            var response = new UserListResponseModel
            {
                Users = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
            return Result<UserListResponseModel>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<UserListResponseModel>.Failure($"Error retrieving users: {ex.Message}");
        }
    }

    public async Task<Result<UserResponseModel>> UpdateUserAsync(string userId, UserRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(userId)) return Result<UserResponseModel>.ValidationError("UserId is required.");
        if (string.IsNullOrWhiteSpace(request.UserName)) return Result<UserResponseModel>.ValidationError("UserName is required.");

        // IDbTransaction transaction = null;
        try
        {
            // First, get the UserCode for the given UserId, as UserCode is used in Tbl_UserTechStack
            var existingUser = await _sqlService.QuerySingleAsync<UserModel>(UserQueries.GetUserById, new { UserId = userId });
            if (existingUser == null)
            {
                return Result<UserResponseModel>.NotFoundError($"User with ID {userId} not found.");
            }
            string userCode = existingUser.UserCode;

            // transaction = await _sqlService.BeginTransactionAsync();

            var userToUpdate = new UserModel // Use UserModel or an anonymous type that matches query params
            {
                UserId = userId,
                UserName = request.UserName,
                GitHubAccountName = request.GitHubAccountName,
                Nrc = request.Nrc,
                MobileNo = request.MobileNo
            };
            int rowsAffected = await _sqlService.ExecuteAsync(UserQueries.UpdateUser, userToUpdate /*, transaction*/);

            // Even if user details weren't changed, tech stacks might have.
            // So, proceed to update tech stacks. If rowsAffected is 0 from user update AND tech stacks are unchanged,
            // then it could be considered "not found" or "no change". Current logic updates stacks regardless.

            // Update Tech Stacks: Delete existing and insert new ones
            await _sqlService.ExecuteAsync(UserQueries.DeleteUserTechStacksByUserCode, new { UserCode = userCode } /*, transaction*/);

            if (request.TechStackCodes != null && request.TechStackCodes.Any())
            {
                foreach (var techStackCode in request.TechStackCodes)
                {
                    var userTechStack = new
                    {
                        UserTechStackId = GenerateUserTechStackId(),
                        UserCode = userCode,
                        TechStackCode = techStackCode,
                        ProficiencyLevel = (int?)null // Or handle proficiency if UI supports it
                    };
                    await _sqlService.ExecuteAsync(UserQueries.InsertUserTechStack, userTechStack /*, transaction*/);
                }
            }

            // await _sqlService.CommitTransactionAsync(transaction);

            var updatedUserWithStacks = await GetUserWithTechStacks(userId);
            if (!updatedUserWithStacks.IsSuccess) return Result<UserResponseModel>.Failure("User updated, but failed to retrieve details.");

            var response = new UserResponseModel
            {
                Users = new List<UserWithTechStacksModel> { new() { User = updatedUserWithStacks.Data } },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };
            return Result<UserResponseModel>.Success(response);
        }
        catch (Exception ex)
        {
            // await _sqlService.RollbackTransactionAsync(transaction);
            return Result<UserResponseModel>.Failure($"Error updating user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<bool>.ValidationError("UserId is required.");
        }

        // IDbTransaction transaction = null;
        try
        {
            var user = await _sqlService.QuerySingleAsync<UserModel>(UserQueries.GetUserById, new { UserId = userId });
            if (user == null)
            {
                return Result<bool>.NotFoundError($"User with ID {userId} not found.");
            }

            // transaction = await _sqlService.BeginTransactionAsync();

            // Delete associated tech stacks first
            await _sqlService.ExecuteAsync(UserQueries.DeleteUserTechStacksByUserCode, new { UserCode = user.UserCode } /*, transaction*/);

            // Then delete the user
            int rowsAffected = await _sqlService.ExecuteAsync(UserQueries.DeleteUser, new { UserId = userId } /*, transaction*/);

            // await _sqlService.CommitTransactionAsync(transaction);

            return rowsAffected > 0
                ? Result<bool>.Success(true, "User deleted successfully.")
                : Result<bool>.NotFoundError($"User with ID {userId} not found or already deleted."); // Should be caught by initial check
        }
        catch (Exception ex)
        {
            // await _sqlService.RollbackTransactionAsync(transaction);
            return Result<bool>.Failure($"Error deleting user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddUsersAsync(List<UserRequestModel> users)
    {
        if (users == null || !users.Any())
        {
            return Result<bool>.ValidationError("No users provided to add.");
        }

        // IDbTransaction transaction = null; // For batch transaction
        try
        {
            // transaction = await _sqlService.BeginTransactionAsync(); // Start one transaction for all users

            foreach (var userRequest in users)
            {
                // UserCode for UserRequestModel is not needed here as CreateUserAsync handles it
                // Ensure UserRequestModel for CreateUserAsync has TechStackCodes
                var createUserResult = await CreateUserAsync(userRequest /*, transaction */); // Pass transaction if CreateUserAsync is refactored to accept it
                if (!createUserResult.IsSuccess)
                {
                    // await _sqlService.RollbackTransactionAsync(transaction);
                    // Aggregate errors or return first error
                    return Result<bool>.Failure($"Failed to add user {userRequest.UserName}: {createUserResult.Message}");
                }
            }

            // await _sqlService.CommitTransactionAsync(transaction);
            return Result<bool>.Success(true, $"{users.Count} users added successfully.");
        }
        catch (Exception ex)
        {
            // await _sqlService.RollbackTransactionAsync(transaction);
            return Result<bool>.Failure($"Error adding users: {ex.Message}");
        }
    }
}