namespace xhunter74.CollectionManager.Data.Entity;

public class File : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CollectionId { get; set; }

    public Collection Collection { get; set; } = null!;
}
