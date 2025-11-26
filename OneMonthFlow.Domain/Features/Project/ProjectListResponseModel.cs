namespace OneMonthFlow.Domain.Features.Project;

public class ProjectListResponseModel
{
    public List<ProjectModel> Projects { get; set; } // Only projects, no teams
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}