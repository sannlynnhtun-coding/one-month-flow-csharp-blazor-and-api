namespace OneMonthFlow.Domain.Features.Team;

public class TeamListResponseModel
{
    public List<TeamModel> Teams { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}