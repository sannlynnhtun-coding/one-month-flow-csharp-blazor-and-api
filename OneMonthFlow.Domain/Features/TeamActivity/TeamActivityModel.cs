

// TeamActivityModel.cs
namespace OneMonthFlow.Domain.Features.TeamActivity;

public class TeamActivityModel
{
    public string ProjectTeamActivityId { get; set; } = Guid.NewGuid().ToString();
    public string UserCode { get; set; } = string.Empty;
    public string TeamCode { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public string? TechStackCode { get; set; }
    public DateTime ActivityDate { get; set; }
    public string Tasks { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}