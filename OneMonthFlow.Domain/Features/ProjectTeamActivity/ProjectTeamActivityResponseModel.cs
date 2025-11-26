namespace OneMonthFlow.Domain.Features.ProjectTeamActivity;

public class ProjectTeamActivityResponseModel
{
    public List<ProjectTeamActivityWithDetailsModel> Activities { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}