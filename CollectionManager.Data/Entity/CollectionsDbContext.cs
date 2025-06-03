using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace xhunter74.CollectionManager.Data.Entity;

public class CollectionsDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public CollectionsDbContext(DbContextOptions<CollectionsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseOpenIddict<Guid>();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var insertedEntries = ChangeTracker.Entries()
                               .Where(x => x.State == EntityState.Added)
                               .Select(x => x.Entity);

        foreach (var insertedEntry in insertedEntries)
        {
            if (insertedEntry is IBaseEntity baseEntity)
            {
                baseEntity.Created = DateTime.UtcNow;
                baseEntity.Updated = DateTime.UtcNow;
            }
        }

        var modifiedEntries = ChangeTracker.Entries()
                   .Where(x => x.State == EntityState.Modified)
                   .Select(x => x.Entity);

        foreach (var modifiedEntry in modifiedEntries)
        {
            //If the inserted object is an Auditable. 
            if (modifiedEntry is IBaseEntity baseEntity)
            {
                baseEntity.Updated = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
