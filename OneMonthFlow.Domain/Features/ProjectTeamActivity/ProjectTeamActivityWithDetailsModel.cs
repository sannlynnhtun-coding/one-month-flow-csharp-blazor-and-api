namespace OneMonthFlow.Domain.Features.ProjectTeamActivity;

public class ProjectTeamActivityWithDetailsModel
{
    public ProjectTeamActivityModel Activity { get; set; } = new();
    public string UserName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string TechStackName { get; set; } = string.Empty;
}