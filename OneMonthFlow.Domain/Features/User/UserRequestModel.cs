namespace OneMonthFlow.Domain.Features.User;

public class UserRequestModel
{
    public string UserCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? GitHubAccountName { get; set; }
    public string? Nrc { get; set; }
    public string? MobileNo { get; set; }
    public List<string> TechStackCodes { get; set; } = new List<string>();
}