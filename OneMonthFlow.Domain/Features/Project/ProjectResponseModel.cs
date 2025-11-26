namespace OneMonthFlow.Domain.Features.Project;

public class ProjectResponseModel
{
    public List<ProjectWithTeamsModel> Projects { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string ProjectId { get; set; }
    public string ProjectName { get; set; }
    public string ProjectCode { get; set; }
}