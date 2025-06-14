using System.ComponentModel.DataAnnotations;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Models;

public record CollectionFieldDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; }
    public FieldTypes Type { get; init; }
    public bool IsSystem { get; set; }
    public bool IsRequired { get; init; }
    public int Order { get; init; }
    public Guid CollectionId { get; init; }
}

public record CreateCollectionFieldDto(
    [Required] string DisplayName,
    [Required] FieldTypes Type,
    int Order
);

public record UpdateCollectionFieldDto(
    [Required] string DisplayName,
    [Required] FieldTypes Type,
    bool IsRequired,
    int Order
);