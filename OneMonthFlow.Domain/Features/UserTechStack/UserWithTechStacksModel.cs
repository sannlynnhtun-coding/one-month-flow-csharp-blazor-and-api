namespace OneMonthFlow.Domain.Features.UserTechStack;

public class UserWithTechStacksModel
{
    public UserModel User { get; set; } = new();
    public List<UserTechStackModel> TechStacks { get; set; } = new();
}