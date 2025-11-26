namespace OneMonthFlow.Domain.Features.TeamUser;

public class TeamWithUsersModel
{
    public TeamModel Team { get; set; } = new();
    public List<TeamUserModel> TeamUsers { get; set; } = new();
    public List<UserModel> Users { get; set; } = new(); // Or TeamUserModel if needed
}