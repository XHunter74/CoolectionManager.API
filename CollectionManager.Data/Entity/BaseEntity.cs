namespace xhunter74.CollectionManager.Data.Entity;

public abstract class BaseEntity : IBaseEntity
{
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}
