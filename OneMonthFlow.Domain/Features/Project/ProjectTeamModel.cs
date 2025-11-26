namespace OneMonthFlow.Domain.Features.Project;

public class ProjectTeamModel
{
    public string ProjectTeamId { get; set; }
    public string ProjectCode { get; set; }
    public string TeamCode { get; set; }
    public string TechStackCode { get; set; }
    public decimal? ProjectTeamRating { get; set; }
    public int? Duration { get; set; }
}