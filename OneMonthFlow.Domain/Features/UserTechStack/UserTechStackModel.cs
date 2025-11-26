namespace OneMonthFlow.Domain.Features.UserTechStack;

public class UserTechStackModel
{
    public string UserTechStackId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string TechStackId { get; set; } = string.Empty;
    public string TechStackName { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string TechStackCode { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}