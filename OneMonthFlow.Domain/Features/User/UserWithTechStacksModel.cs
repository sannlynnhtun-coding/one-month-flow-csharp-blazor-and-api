using OneMonthFlow.Domain.Features.UserTechStack;

namespace OneMonthFlow.Domain.Features.User;

public class UserWithTechStacksModel
{
    public UserModel User { get; set; } = new();
    public List<UserTechStackModel> TechStacks { get; set; } = new();
}