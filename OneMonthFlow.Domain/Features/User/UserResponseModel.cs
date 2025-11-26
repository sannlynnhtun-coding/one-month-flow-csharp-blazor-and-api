namespace OneMonthFlow.Domain.Features.User;

public class UserResponseModel
{
    public List<UserWithTechStacksModel> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}