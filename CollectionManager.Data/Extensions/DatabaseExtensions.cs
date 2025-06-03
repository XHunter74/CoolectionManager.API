using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.Data.Extensions;

public static class DatabaseExtensions
{
    public static IHost ApplyDbMigrations(this IHost host)
    {
        using var serviceScope = host.Services.CreateScope();
        using var context = (DbContext)serviceScope.ServiceProvider.GetRequiredService<CollectionsDbContext>();

        context.Database.Migrate();

        return host;
    }
}
