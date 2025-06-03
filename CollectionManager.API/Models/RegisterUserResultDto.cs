namespace xhunter74.CollectionManager.API.Models;

public record RegisterUserResultDto
{
	public Guid Id { get; init; }
	public string Name { get; init; }
}
