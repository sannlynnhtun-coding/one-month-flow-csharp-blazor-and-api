namespace OneMonthFlow.Domain.Features.TeamUser;

public class TeamUserListResponseModel
{
    public List<TeamUserModel> TeamUsers { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}