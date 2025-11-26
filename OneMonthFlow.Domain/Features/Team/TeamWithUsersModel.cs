namespace OneMonthFlow.Domain.Features.Team;

public class TeamWithUsersModel
{
    public TeamModel Team { get; set; } = new();
    public List<TeamUserModel> Users { get; set; } = new();
}