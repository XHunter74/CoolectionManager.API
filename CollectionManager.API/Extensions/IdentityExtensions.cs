using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.API.Settings;

namespace xhunter74.CollectionManager.API.Extensions;

public static class IdentityExtensions
{
    public static void SeedIdentityEntities(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        using var context = (DbContext)scope.ServiceProvider.GetRequiredService<CollectionsDbContext>();
        var identitySettings = scope.ServiceProvider.GetRequiredService<IOptions<IdentitySettings>>().Value;

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var existingClientApp = manager.FindByClientIdAsync(identitySettings.DefaultClientId).GetAwaiter().GetResult();
        if (existingClientApp == null)
        {
            manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = identitySettings.DefaultClientId,
                ClientSecret = identitySettings.DefaultClientSecret,
                DisplayName = "Default client application",
                Permissions =
                    {
                        OpenIddictConstants.Permissions.Endpoints.Token,
                        OpenIddictConstants.Permissions.GrantTypes.Password,
                        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    }
            }).GetAwaiter().GetResult();
        }
    }
}
