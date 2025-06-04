using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace xhunter74.CollectionManager.Data.Entity;

public class CollectionsDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public virtual DbSet<Collection> Collections { get; set; }
    public virtual DbSet<CollectionField> CollectionFields { get; set; }

    public CollectionsDbContext(DbContextOptions<CollectionsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.UseOpenIddict<Guid>();

        builder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.Collections)
                .HasForeignKey(e => e.OwnerId)
                .HasPrincipalKey(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CollectionField>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired();
            entity.HasOne(e => e.Collection)
                .WithMany(c => c.Fields)
                .HasForeignKey(e => e.CollectionId)
                .HasPrincipalKey(e => e.Id)
                .OnDelete(DeleteBehavior.Cascade);
        });
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
