namespace OneMonthFlow.Domain.Features.TeamUser;

public class TeamUserService
{
    private readonly ISqlService _sqlService;

    public TeamUserService(ISqlService sqlService) => _sqlService = sqlService;

    public async Task<Result<TeamUserResponseModel>> AddTeamUserAsync(TeamUserRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.TeamCode) || string.IsNullOrWhiteSpace(request.UserCode))
            {
                return Result<TeamUserResponseModel>.ValidationError("TeamCode and UserCode are required.");
            }

            // Check if team exists
            var teamExists = await _sqlService.QuerySingleAsync<bool>(
                TeamUserQueries.CheckTeamExists, new { TeamCode = request.TeamCode });
            if (!teamExists)
            {
                return Result<TeamUserResponseModel>.NotFoundError($"Team with code {request.TeamCode} not found.");
            }

            // Check if user exists
            var userExists = await _sqlService.QuerySingleAsync<bool>(
                TeamUserQueries.CheckUserExists, new { UserCode = request.UserCode });
            if (!userExists)
            {
                return Result<TeamUserResponseModel>.NotFoundError($"User with code {request.UserCode} not found.");
            }

            // Check if team-user combination already exists
            var existingTeamUser = await _sqlService.QueryFirstOrDefaultAsync<TeamUserModel>(
                TeamUserQueries.GetTeamUserByTeamAndUser,
                new { TeamCode = request.TeamCode, UserCode = request.UserCode });

            if (existingTeamUser != null)
            {
                // Instead of error, could be a success if idempotent add is desired.
                // For now, treating as "already exists" is an issue if trying to add anew.
                return Result<TeamUserResponseModel>.Success(new(), $"User {request.UserCode} is already a member of team {request.TeamCode}.");
            }

            var teamUser = new TeamUserModel
            {
                TeamUserId = Ulid.NewUlid().ToString(),
                TeamCode = request.TeamCode,
                UserCode = request.UserCode,
                UserRating = request.UserRating // Assuming UserRating is part of TeamUserRequestModel
            };

            int rowsAffected = await _sqlService.ExecuteAsync(TeamUserQueries.InsertTeamUser, teamUser);
            return rowsAffected > 0
                ? Result<TeamUserResponseModel>.Success(new(), $"User {request.UserCode} added to team {request.TeamCode}.")
                : Result<TeamUserResponseModel>.Failure("Failed to add user to team. No rows affected.");
        }
        catch (Exception ex)
        {
            // Log exception (ex)
            return Result<TeamUserResponseModel>.Failure($"Error adding team user: {ex.Message}");
        }
    }

    // This method returns TeamUserModel instances, used for different purposes than GetUsersByTeamCodeAsync
    public async Task<Result<TeamUserResponseModel>> GetTeamUsersByTeamAsync(string teamCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamCode))
            {
                return Result<TeamUserResponseModel>.ValidationError("TeamCode is required.");
            }

            // This query should ideally map to TeamUserModel which might include UserRating, etc.
            var teamUsers = await _sqlService.QueryAsync<TeamUserModel>(
                TeamUserQueries.GetTeamUserModelsWithDetailsByTeamCode, new { TeamCode = teamCode });

            var response = new TeamUserResponseModel
            {
                TeamUsers = teamUsers.ToList(),
                TotalCount = teamUsers.Count(),
                Page = 1, // Not paginated here
                PageSize = teamUsers.Count() // Not paginated here
            };

            return Result<TeamUserResponseModel>.Success(response);
        }
        catch (Exception ex)
        {
            // Log exception (ex)
            return Result<TeamUserResponseModel>.Failure($"Error retrieving team user entries: {ex.Message}");
        }
    }

    // New method as required by the Blazor component
    public async Task<Result<List<UserModel>>> GetUsersByTeamCodeAsync(string teamCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamCode))
            {
                return Result<List<UserModel>>.ValidationError("TeamCode is required.");
            }

            var users = await _sqlService.QueryAsync<UserModel>(
                TeamUserQueries.GetUserModelsByTeamCode, new { TeamCode = teamCode });

            return Result<List<UserModel>>.Success(users.ToList());
        }
        catch (Exception ex)
        {
            // Log exception (ex)
            return Result<List<UserModel>>.Failure($"Error retrieving users for team {teamCode}: {ex.Message}");
        }
    }


    public async Task<Result<TeamUserListResponseModel>> GetAllTeamUsersAsync(int page = 1, int pageSize = 10, string? filterColumn = null, string? filterValue = null)
    {
        try
        {
            if (page < 1 || pageSize < 1)
            {
                return Result<TeamUserListResponseModel>.ValidationError("Page and PageSize must be greater than 0.");
            }

            var offset = (page - 1) * pageSize;
            string effectiveFilterValue = string.IsNullOrWhiteSpace(filterValue) ? "%" : $"%{filterValue}%";

            var parameters = new
            {
                PageSize = pageSize,
                Offset = offset,
                FilterValue = effectiveFilterValue
            };

            // Ensure TeamUserQueries.GetTeamUsersPaginated and GetTeamUserCount handle the filter correctly
            var teamUsers = await _sqlService.QueryAsync<TeamUserModel>(
                TeamUserQueries.GetTeamUsersPaginated, parameters);

            var totalCountParameters = new { FilterValue = effectiveFilterValue };
            var totalCount = await _sqlService.QuerySingleAsync<int>(
                TeamUserQueries.GetTeamUserCount, totalCountParameters);

            var response = new TeamUserListResponseModel
            {
                TeamUsers = teamUsers.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<TeamUserListResponseModel>.Success(response);
        }
        catch (Exception ex)
        {
            // Log exception (ex)
            return Result<TeamUserListResponseModel>.Failure($"Error retrieving all team users: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveTeamUserAsync(string teamUserId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamUserId))
            {
                return Result<bool>.ValidationError("TeamUserId is required.");
            }

            int rowsAffected = await _sqlService.ExecuteAsync(TeamUserQueries.DeleteTeamUser, new { TeamUserId = teamUserId });

            return rowsAffected > 0
                ? Result<bool>.Success(true, "Team user removed successfully.")
                : Result<bool>.NotFoundError($"TeamUser with ID {teamUserId} not found or already removed.");
        }
        catch (Exception ex)
        {
            // Log exception (ex)
            return Result<bool>.Failure($"Error removing team user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveUserFromTeamAsync(string teamCode, string userCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamCode) || string.IsNullOrWhiteSpace(userCode))
            {
                return Result<bool>.ValidationError("TeamCode and UserCode are required.");
            }

            int rowsAffected = await _sqlService.ExecuteAsync(TeamUserQueries.DeleteTeamUserByTeamAndUserCode,
                                                            new { TeamCode = teamCode, UserCode = userCode });
            return rowsAffected > 0
                ? Result<bool>.Success(true, $"User {userCode} removed from team {teamCode}.")
                : Result<bool>.NotFoundError($"User {userCode} not found in team {teamCode}, or already removed.");
        }
        catch (Exception ex)
        {
            // Log exception (ex)
            return Result<bool>.Failure($"Error removing user {userCode} from team {teamCode}: {ex.Message}");
        }
    }


    public async Task<Result<TeamWithUsersModel>> GetTeamWithUsersAsync(string teamCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamCode))
            {
                return Result<TeamWithUsersModel>.ValidationError("TeamCode is required.");
            }

            var team = await _sqlService.QueryFirstOrDefaultAsync<TeamModel>( // Use QuerySingleOrDefaultAsync
                TeamUserQueries.GetTeamByCode, new { TeamCode = teamCode });

            if (team == null)
            {
                return Result<TeamWithUsersModel>.NotFoundError($"Team with code {teamCode} not found.");
            }

            // Use the new GetUsersByTeamCodeAsync to get UserModel list
            var usersResult = await GetUsersByTeamCodeAsync(teamCode);
            if (!usersResult.IsSuccess || usersResult.Data == null)
            {
                // If GetUsersByTeamCodeAsync failed, use its message. Otherwise, a generic one.
                return Result<TeamWithUsersModel>.Failure(usersResult.Message ?? "Failed to retrieve users for the team.");
            }

            var response = new TeamWithUsersModel
            {
                Team = team,
                Users = usersResult.Data
            };

            return Result<TeamWithUsersModel>.Success(response);
        }
        catch (Exception ex)
        {
            // Log exception (ex)
            return Result<TeamWithUsersModel>.Failure($"Error retrieving team with users: {ex.Message}");
        }
    }

    public async Task<Result<List<UserModel>>> SearchUsersAsync(string query)
    {
        try
        {
            // Ensure query parameter is correctly passed to SQL
            string sqlQuery = string.IsNullOrWhiteSpace(query) ? $"%{""}%" : $"%{query}%";

            var users = await _sqlService.QueryAsync<UserModel>(TeamUserQueries.SearchUsers, new { Query = sqlQuery });
            return Result<List<UserModel>>.Success(users.ToList());
        }
        catch (Exception ex)
        {
            // Log exception (ex)
            return Result<List<UserModel>>.Failure($"Error searching users: {ex.Message}");
        }
    }

    public async Task<Result<List<TeamModel>>> SearchTeamsAsync(string query)
    {
        try
        {
            string sqlQuery = string.IsNullOrWhiteSpace(query) ? $"%{""}%" : $"%{query}%";

            var teams = await _sqlService.QueryAsync<TeamModel>(TeamUserQueries.SearchTeams, new { Query = sqlQuery });
            return Result<List<TeamModel>>.Success(teams.OrderBy(x => x.TeamCode).ToList());
        }
        catch (Exception ex)
        {
            // Log exception (ex)
            return Result<List<TeamModel>>.Failure($"Error searching teams: {ex.Message}");
        }
    }
}
