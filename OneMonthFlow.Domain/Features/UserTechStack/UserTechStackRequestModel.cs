namespace OneMonthFlow.Domain.Features.UserTechStack;

public class UserTechStackRequestModel
{
    public string? UserTechStackId { get; set; } = string.Empty!;
    public string UserCode { get; set; } = string.Empty;
    public string TechStackCode { get; set; } = string.Empty;
    public int ProficiencyLevel { get; set; }
}