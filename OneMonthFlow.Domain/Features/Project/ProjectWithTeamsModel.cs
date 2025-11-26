namespace OneMonthFlow.Domain.Features.Project;

public class ProjectWithTeamsModel
{
    public ProjectModel Project { get; set; }
    public List<ProjectTeamModel> Teams { get; set; }
}