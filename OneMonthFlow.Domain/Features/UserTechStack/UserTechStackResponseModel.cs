namespace OneMonthFlow.Domain.Features.UserTechStack;

public class UserTechStackResponseModel
{
    public List<UserTechStackModel> UserTechStacks { get; set; } = new();
    public List<UserModel> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}