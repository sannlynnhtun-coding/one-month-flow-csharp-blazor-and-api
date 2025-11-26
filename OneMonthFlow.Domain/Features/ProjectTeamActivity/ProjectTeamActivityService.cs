namespace OneMonthFlow.Domain.Features.ProjectTeamActivity;

public class ProjectTeamActivityService
{
    private readonly ISqlService _sqlService;

    public ProjectTeamActivityService(ISqlService sqlService)
    {
        _sqlService = sqlService ?? throw new ArgumentNullException(nameof(sqlService));
    }

    public async Task<Result<IEnumerable<UserModel>>> SearchUsersAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Result<IEnumerable<UserModel>>.Success(Enumerable.Empty<UserModel>());
            }
            var parameters = new { SearchPattern = $"%{searchTerm.ToLower()}%", SearchTerm = searchTerm };
            var users = await _sqlService.QueryAsync<UserModel>(ProjectTeamActivityQueries.SearchUsers, parameters);
            return Result<IEnumerable<UserModel>>.Success(users ?? Enumerable.Empty<UserModel>());
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<UserModel>>.Failure($"Error searching users: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<TechStackModel>>> SearchTechStacksAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Result<IEnumerable<TechStackModel>>.Success(Enumerable.Empty<TechStackModel>());
            }
            var parameters = new { SearchPattern = $"%{searchTerm.ToLower()}%", SearchTerm = searchTerm };
            var techStacks = await _sqlService.QueryAsync<TechStackModel>(ProjectTeamActivityQueries.SearchTechStacks, parameters);
            return Result<IEnumerable<TechStackModel>>.Success(techStacks ?? Enumerable.Empty<TechStackModel>());
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<TechStackModel>>.Failure($"Error searching tech stacks: {ex.Message}");
        }
    }

    public async Task<Result<TechStackModel>> GetTechStackByCodeAsync(string techStackCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(techStackCode))
            {
                return Result<TechStackModel>.ValidationError("TechStackCode is required.");
            }
            var techStack = await _sqlService.QueryFirstOrDefaultAsync<TechStackModel>(
                ProjectTeamActivityQueries.GetTechStackByCode, new { TechStackCode = techStackCode });

            return techStack != null
                ? Result<TechStackModel>.Success(techStack)
                : Result<TechStackModel>.NotFoundError($"Tech stack with code {techStackCode} not found.");
        }
        catch (Exception ex)
        {
            return Result<TechStackModel>.Failure($"Error retrieving tech stack by code: {ex.Message}");
        }
    }

    public async Task<Result<UserModel>> GetUserByCodeAsync(string userCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<UserModel>.ValidationError("UserCode is required.");
            }
            var user = await _sqlService.QueryFirstOrDefaultAsync<UserModel>(
                ProjectTeamActivityQueries.GetUserByCode, new { UserCode = userCode });

            return user != null
                ? Result<UserModel>.Success(user)
                : Result<UserModel>.NotFoundError($"User with code {userCode} not found.");
        }
        catch (Exception ex)
        {
            return Result<UserModel>.Failure($"Error retrieving user by code: {ex.Message}");
        }
    }

    public async Task<Result<ProjectTeamActivityModel>> AddActivityAsync(ProjectTeamActivityModel activity)
    {
        try
        {
            if (activity == null ||
                string.IsNullOrWhiteSpace(activity.TeamCode) ||
                string.IsNullOrWhiteSpace(activity.ProjectCode) ||
                activity.ActivityDate == null ||
                string.IsNullOrWhiteSpace(activity.Tasks) ||
                string.IsNullOrWhiteSpace(activity.UserCode) ||      // Assuming UserCode is now required
                string.IsNullOrWhiteSpace(activity.TechStackCode))   // Assuming TechStackCode is now required
            {
                return Result<ProjectTeamActivityModel>.ValidationError("UserCode, TechStackCode, TeamCode, ProjectCode, ActivityDate, and Tasks are required.");
            }

            activity.ProjectTeamActivityId = Ulid.NewUlid().ToString();

            int rowsAffected = await _sqlService.ExecuteAsync(ProjectTeamActivityQueries.InsertProjectTeamActivity, activity);

            return rowsAffected > 0
                ? Result<ProjectTeamActivityModel>.Success(activity, "Activity logged successfully.")
                : Result<ProjectTeamActivityModel>.Failure("Failed to log activity. No rows affected.");
        }
        catch (Exception ex)
        {
            return Result<ProjectTeamActivityModel>.Failure($"Error logging activity: {ex.Message}");
        }
    }

    public async Task<Result<ProjectTeamActivityModel>> UpdateActivityAsync(ProjectTeamActivityModel activity)
    {
        try
        {
            if (activity == null || string.IsNullOrWhiteSpace(activity.ProjectTeamActivityId))
            {
                return Result<ProjectTeamActivityModel>.ValidationError("ProjectTeamActivityId is required for update.");
            }
            if (string.IsNullOrWhiteSpace(activity.TeamCode) ||
                string.IsNullOrWhiteSpace(activity.ProjectCode) ||
                activity.ActivityDate == null ||
                string.IsNullOrWhiteSpace(activity.Tasks) ||
                string.IsNullOrWhiteSpace(activity.UserCode) ||      // Assuming UserCode is now required
                string.IsNullOrWhiteSpace(activity.TechStackCode))   // Assuming TechStackCode is now required
            {
                return Result<ProjectTeamActivityModel>.ValidationError("UserCode, TechStackCode, TeamCode, ProjectCode, ActivityDate, and Tasks are required.");
            }

            int rowsAffected = await _sqlService.ExecuteAsync(ProjectTeamActivityQueries.UpdateProjectTeamActivity, activity);

            return rowsAffected > 0
                ? Result<ProjectTeamActivityModel>.Success(activity, "Activity updated successfully.")
                : Result<ProjectTeamActivityModel>.NotFoundError("Activity not found or no changes made.");
        }
        catch (Exception ex)
        {
            return Result<ProjectTeamActivityModel>.Failure($"Error updating activity: {ex.Message}");
        }
    }

    public async Task<Result<List<ProjectTeamActivityModel>>> GetActivitiesAsync(string teamCodeFilter, string projectCodeFilter)
    {
        try
        {
            var sqlBuilder = new StringBuilder(ProjectTeamActivityQueries.GetActivitiesBase);
            var parameters = new Dictionary<string, object>();
            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(teamCodeFilter))
            {
                conditions.Add("pta.TeamCode = @TeamCode");
                parameters["TeamCode"] = teamCodeFilter;
            }
            if (!string.IsNullOrWhiteSpace(projectCodeFilter))
            {
                conditions.Add("pta.ProjectCode = @ProjectCode");
                parameters["ProjectCode"] = projectCodeFilter;
            }

            if (conditions.Any())
            {
                sqlBuilder.Append(" WHERE ");
                sqlBuilder.Append(string.Join(" AND ", conditions));
            }
            sqlBuilder.Append(" ORDER BY pta.ActivityDate DESC;");

            string finalQuery = sqlBuilder.ToString();
            var activities = await _sqlService.QueryAsync<ProjectTeamActivityModel>(finalQuery, parameters);
            return Result<List<ProjectTeamActivityModel>>.Success(activities?.ToList() ?? new List<ProjectTeamActivityModel>());
        }
        catch (Exception ex)
        {
            return Result<List<ProjectTeamActivityModel>>.Failure($"Error retrieving activities: {ex.Message}");
        }
    }

    public async Task<Result<ProjectTeamActivityModel>> GetActivityByIdAsync(string activityId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(activityId))
            {
                return Result<ProjectTeamActivityModel>.ValidationError("Activity ID is required.");
            }
            var activity = await _sqlService.QueryFirstOrDefaultAsync<ProjectTeamActivityModel>(
                ProjectTeamActivityQueries.GetProjectTeamActivityById,
                new { ProjectTeamActivityId = activityId });

            return activity == null
                ? Result<ProjectTeamActivityModel>.NotFoundError($"Activity with ID {activityId} not found.")
                : Result<ProjectTeamActivityModel>.Success(activity);
        }
        catch (Exception ex)
        {
            return Result<ProjectTeamActivityModel>.Failure($"Error fetching activity by ID: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteActivityAsync(string activityId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(activityId))
            {
                return Result<bool>.ValidationError("ProjectTeamActivityId is required.");
            }

            int rowsAffected = await _sqlService.ExecuteAsync(ProjectTeamActivityQueries.DeleteProjectTeamActivity, new { ProjectTeamActivityId = activityId });

            return rowsAffected > 0
                ? Result<bool>.Success(true, "Activity deleted successfully.")
                : Result<bool>.NotFoundError($"Activity with ID {activityId} not found or already deleted.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting activity: {ex.Message}");
        }
    }

    public async Task<Result<TeamModel>> GetTeamByCodeAsync(string teamCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamCode))
            {
                return Result<TeamModel>.ValidationError("TeamCode is required.");
            }
            var team = await _sqlService.QueryFirstOrDefaultAsync<TeamModel>(
                ProjectTeamActivityQueries.GetTeamByCodeQuery, new { TeamCode = teamCode });

            return team == null
                ? Result<TeamModel>.NotFoundError($"Team with code {teamCode} not found.")
                : Result<TeamModel>.Success(team);
        }
        catch (Exception ex)
        {
            return Result<TeamModel>.Failure($"Error fetching team by code: {ex.Message}");
        }
    }

    public async Task<Result<List<ProjectModel>>> GetProjectsByTeamCodeAsync(string teamCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamCode))
            {
                return Result<List<ProjectModel>>.ValidationError("TeamCode is required to fetch projects.");
            }
            var projects = await _sqlService.QueryAsync<ProjectModel>(
                ProjectTeamActivityQueries.GetProjectsByTeamCodeQuery, new { TeamCode = teamCode });
            return Result<List<ProjectModel>>.Success(projects?.ToList() ?? new List<ProjectModel>());
        }
        catch (Exception ex)
        {
            return Result<List<ProjectModel>>.Failure($"Error fetching projects for team {teamCode}: {ex.Message}");
        }
    }

    public async Task<Result<List<TeamModel>>> SearchTeamsAsync(string query)
    {
        try
        {
            string sqlQueryParam = string.IsNullOrWhiteSpace(query) ? "%" : $"%{query.ToLower()}%";
            var teams = await _sqlService.QueryAsync<TeamModel>(
                ProjectTeamActivityQueries.SearchTeamsQuery, new { Query = sqlQueryParam });
            return Result<List<TeamModel>>.Success(teams?.ToList() ?? new List<TeamModel>());
        }
        catch (Exception ex)
        {
            return Result<List<TeamModel>>.Failure($"Error searching teams: {ex.Message}");
        }
    }
}