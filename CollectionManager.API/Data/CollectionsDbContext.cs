using Microsoft.EntityFrameworkCore;

namespace xhunter74.CollectionManager.API.Data;

public class CollectionsDbContext : DbContext
{
    public CollectionsDbContext(DbContextOptions<CollectionsDbContext> options)
   : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
    }
}
