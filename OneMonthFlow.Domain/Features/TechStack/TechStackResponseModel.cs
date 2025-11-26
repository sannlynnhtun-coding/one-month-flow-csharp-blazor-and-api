namespace OneMonthFlow.Domain.Features.TechStack;

public class TechStackResponseModel
{
    public List<TechStackModel> TechStacks { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}