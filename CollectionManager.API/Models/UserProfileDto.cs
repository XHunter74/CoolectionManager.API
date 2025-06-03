namespace xhunter74.CollectionManager.API.Models;

public record UserProfileDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public Guid? Avatar { get; init; }
}
