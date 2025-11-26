namespace OneMonthFlow.Domain.Features.TechStack;

public class TechStackModel
{
    public string TechStackId { get; set; } = Guid.NewGuid().ToString();
    public string TechStackCode { get; set; } = string.Empty;
    public string TechStackShortCode { get; set; } = string.Empty;
    public string TechStackName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}