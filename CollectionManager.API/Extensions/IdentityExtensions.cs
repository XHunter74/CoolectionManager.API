using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using xhunter74.CollectionManager.API.Permissions;
using xhunter74.CollectionManager.API.Settings;
using xhunter74.CollectionManager.Data.Entity;

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

    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IdentitySettings>()
            .Bind(configuration.GetSection(IdentitySettings.ConfigSection))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            //TODO Identity options configuration (password, lockout, etc.)
        })
        .AddEntityFrameworkStores<CollectionsDbContext>()
        .AddDefaultTokenProviders();

        var identitySettings = configuration.GetSection(IdentitySettings.ConfigSection).Get<IdentitySettings>();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<CollectionsDbContext>()
                       .ReplaceDefaultEntities<Guid>();

            })
                .AddServer(options =>
                {
                    // Enable the required endpoints
                    options.SetTokenEndpointUris("/connect/token");

                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();

                    options.UseReferenceAccessTokens();
                    options.UseReferenceRefreshTokens();

                    options.RegisterScopes(OpenIddictConstants.Permissions.Scopes.Email,
                                    OpenIddictConstants.Permissions.Scopes.Profile,
                                    OpenIddictConstants.Permissions.Scopes.Roles);

                    options.SetAccessTokenLifetime(TimeSpan.FromSeconds(identitySettings.AccessTokenLifetime));
                    options.SetRefreshTokenLifetime(TimeSpan.FromSeconds(identitySettings.RefreshTokenLifetime));

                    //TODO - Change in production
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();

                    options.UseAspNetCore().EnableTokenEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });
        return services;
    }
}
