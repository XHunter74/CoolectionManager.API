namespace xhunter74.CollectionManager.Data.Entity;

public class Collection : BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid OwnerId { get; set; }

    public ApplicationUser Owner { get; set; }
}
