using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace xhunter74.CollectionManager.API.Data;

public class CollectionsDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public CollectionsDbContext(DbContextOptions<CollectionsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseOpenIddict<Guid>();
    }
}
