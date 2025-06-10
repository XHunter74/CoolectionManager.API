namespace xhunter74.CollectionManager.Data.Entity;

public class CollectionField : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string? Description { get; set; }
    public FieldTypes Type { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public bool IsSystem { get; set; }

    public Guid CollectionId { get; set; }
    public Collection Collection { get; set; }
    public ICollection<PossibleValue> PossibleValues { get; set; } = new List<PossibleValue>();
}
