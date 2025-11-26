namespace OneMonthFlow.Domain.Features.TeamActivity;

public class TeamActivityService
{
    private readonly ISqlService _sqlService;

    public TeamActivityService(ISqlService sqlService)
    {
        _sqlService = sqlService;
    }

    public async Task<Result<bool>> CreateActivityAsync(TeamActivityModel activity)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(activity.UserCode) ||
                string.IsNullOrWhiteSpace(activity.TeamCode) ||
                string.IsNullOrWhiteSpace(activity.ProjectCode))
            {
                return Result<bool>.ValidationError("UserCode, TeamCode, and ProjectCode are required.");
            }

            int rowsAffected = await _sqlService.ExecuteAsync(TeamActivityQueries.InsertTeamActivity, activity);
            return rowsAffected > 0
                ? Result<bool>.Success(true, "Activity logged successfully.")
                : Result<bool>.Failure("Failed to log activity.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error logging activity: {ex.Message}");
        }
    }

    public async Task<Result<List<TeamActivityModel>>> GetActivitiesByTeamCodeAsync(string teamCode)
    {
        try
        {
            var activities = await _sqlService.QueryAsync<TeamActivityModel>(
                TeamActivityQueries.GetActivitiesByTeamCode,
                new { TeamCode = teamCode });

            return Result<List<TeamActivityModel>>.Success(activities.ToList(), $"Found {activities.Count()} activities.");
        }
        catch (Exception ex)
        {
            return Result<List<TeamActivityModel>>.Failure($"Error retrieving activities: {ex.Message}");
        }
    }

    public async Task<Result<TeamActivityModel>> GetActivityByIdAsync(string activityId)
    {
        try
        {
            var activity = await _sqlService.QuerySingleAsync<TeamActivityModel>(
                TeamActivityQueries.GetActivityById,
                new { ProjectTeamActivityId = activityId });

            if (activity == null)
                return Result<TeamActivityModel>.NotFoundError();

            return Result<TeamActivityModel>.Success(activity);
        }
        catch (Exception ex)
        {
            return Result<TeamActivityModel>.Failure($"Error retrieving activity: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateActivityAsync(TeamActivityModel activity)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(activity.ProjectTeamActivityId))
            {
                return Result<bool>.ValidationError("Activity ID is required.");
            }

            int rowsAffected = await _sqlService.ExecuteAsync(TeamActivityQueries.UpdateActivity, activity);
            return rowsAffected > 0
                ? Result<bool>.Success(true, "Activity updated successfully.")
                : Result<bool>.NotFoundError();
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating activity: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteActivityAsync(string activityId)
    {
        try
        {
            int rowsAffected = await _sqlService.ExecuteAsync(TeamActivityQueries.DeleteActivity, new { ProjectTeamActivityId = activityId });
            return rowsAffected > 0
                ? Result<bool>.Success(true, "Activity deleted successfully.")
                : Result<bool>.NotFoundError();
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting activity: {ex.Message}");
        }
    }
}