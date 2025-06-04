using System.ComponentModel.DataAnnotations;

namespace xhunter74.CollectionManager.API.Models;

public record CollectionDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
}

public record CreateCollectionDto([Required] string Name, string Description);

public record UpdateCollectionDto([Required] string Name, string Description);
