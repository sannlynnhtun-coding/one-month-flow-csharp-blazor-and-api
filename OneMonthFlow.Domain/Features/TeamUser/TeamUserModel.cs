namespace OneMonthFlow.Domain.Features.TeamUser;

public class TeamUserModel
{
    public string TeamUserId { get; set; } = Guid.NewGuid().ToString();
    public string TeamCode { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public decimal? UserRating { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}