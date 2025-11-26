namespace OneMonthFlow.Domain.Features.Team;

public class TeamResponseModel
{
    public List<TeamWithUsersModel> Teams { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}