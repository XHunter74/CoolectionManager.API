using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Data;

namespace xhunter74.CollectionManager.API.Extensions;

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
