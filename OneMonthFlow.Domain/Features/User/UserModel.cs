namespace OneMonthFlow.Domain.Features.User;

public class UserModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? GitHubAccountName { get; set; } = string.Empty!;
    public string? Nrc { get; set; }
    public string? MobileNo { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public List<TechStackModel> TechStacks { get; set; } = new List<TechStackModel>();
}