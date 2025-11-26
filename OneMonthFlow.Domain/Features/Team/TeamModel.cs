namespace OneMonthFlow.Domain.Features.Team;

public class TeamModel
{
    public string TeamId { get; set; } = Guid.NewGuid().ToString();
    public string TeamCode { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
