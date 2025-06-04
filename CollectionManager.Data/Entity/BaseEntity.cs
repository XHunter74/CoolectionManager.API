using System.ComponentModel.DataAnnotations;

namespace xhunter74.CollectionManager.Data.Entity;

public abstract class BaseEntity : IBaseEntity
{
    [Required]
    public DateTime Created { get; set; }
    [Required]
    public DateTime Updated { get; set; }
}
