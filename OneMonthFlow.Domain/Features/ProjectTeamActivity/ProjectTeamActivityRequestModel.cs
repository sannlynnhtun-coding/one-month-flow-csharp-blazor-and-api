namespace OneMonthFlow.Domain.Features.ProjectTeamActivity;

public class ProjectTeamActivityRequestModel
{
    public string UserCode { get; set; } = string.Empty;
    public string TeamCode { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string TechStackCode { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
    public string Tasks { get; set; } = string.Empty;
}