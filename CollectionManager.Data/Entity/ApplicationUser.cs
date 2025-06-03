using Microsoft.AspNetCore.Identity;

namespace xhunter74.CollectionManager.Data.Entity;

public class ApplicationUser : IdentityUser<Guid>, IBaseEntity
{
    public Guid? Avatar { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}