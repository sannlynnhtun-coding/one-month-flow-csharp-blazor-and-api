namespace OneMonthFlow.Domain.Features.Team;

public class TeamRequestModel
{
    public string TeamCode { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public List<string> TechStackCodes { get; set; } = new List<string>();
}