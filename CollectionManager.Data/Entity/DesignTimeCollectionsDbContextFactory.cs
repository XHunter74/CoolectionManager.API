using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace xhunter74.CollectionManager.Data.Entity;

public class DesignTimeCollectionsDbContextFactory : IDesignTimeDbContextFactory<CollectionsDbContext>
{
    public CollectionsDbContext CreateDbContext(string[] args)
    {
        const string connectionString = @"User ID=test-user;Server=127.0.0.1;Port=5432;Database=collections;Pooling=true;Password=Test@123";
        var optionsBuilder = new DbContextOptionsBuilder<CollectionsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new CollectionsDbContext(optionsBuilder.Options);
    }
}