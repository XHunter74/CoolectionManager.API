using System.ComponentModel.DataAnnotations;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Models;

public record CollectionFieldDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string? Description { get; init; }
    public FieldTypes Type { get; init; }
    public bool IsSystem { get; set; }
    public bool IsRequired { get; init; }
    public int Order { get; init; }
    public Guid CollectionId { get; init; }
}

public record CreateCollectionFieldDto(
    [Required] string Name,
    string? Description,
    [Required] FieldTypes Type,
    bool IsRequired,
    int Order
);

public record UpdateCollectionFieldDto(
    [Required] string Name,
    string? Description,
    [Required] FieldTypes Type,
    bool IsRequired,
    int Order
);