namespace OneMonthFlow.Domain.Features.Project;

public class ProjectRequestModel
{
    public string ProjectCode { get; set; }
    public string ProjectName { get; set; }
    public string RepoUrl { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ProjectDescription { get; set; }
    public string Status { get; set; }
    public List<ProjectTeamRequestModel> Teams { get; set; }
}