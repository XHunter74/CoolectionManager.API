using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.API.Settings;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using xhunter74.CollectionManager.API.Permissions;

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

        var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole<Guid>>>();

        if (userManager != null && roleManager != null)
        {
            var superAdminEmail = identitySettings.SuperAdminEmail;
            var superAdminPassword = identitySettings.SuperAdminPassword;
            var superAdminRole = "SuperAdmin";

            var roleExists = roleManager.RoleExistsAsync(superAdminRole).GetAwaiter().GetResult();
            if (!roleExists)
            {
                var role = new IdentityRole<Guid> { Name = superAdminRole };
                roleManager.CreateAsync(role).GetAwaiter().GetResult();
            }

            var superAdminUser = userManager.FindByEmailAsync(superAdminEmail).GetAwaiter().GetResult();
            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true
                };
                var result = userManager.CreateAsync(superAdminUser, superAdminPassword).GetAwaiter().GetResult();
                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(superAdminUser, superAdminRole).GetAwaiter().GetResult();
                    userManager.AddClaimAsync(superAdminUser, new Claim(AppClaimTypes.UserPermissionClaim, Permissions.Permissions.ViewUser)).GetAwaiter().GetResult();
                }
            }
            else
            {
                var inRole = userManager.IsInRoleAsync(superAdminUser, superAdminRole).GetAwaiter().GetResult();
                if (!inRole)
                {
                    userManager.AddToRoleAsync(superAdminUser, superAdminRole).GetAwaiter().GetResult();
                }
            }
        }
    }
}
