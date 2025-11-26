namespace OneMonthFlow.Domain.Features.TeamUser;

public class TeamUserRequestModel
{
    public string TeamCode { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public decimal? UserRating { get; set; }
}