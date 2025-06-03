using Microsoft.AspNetCore.Identity;

namespace xhunter74.CollectionManager.Data.Entity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? Avatar { get; set; }
}