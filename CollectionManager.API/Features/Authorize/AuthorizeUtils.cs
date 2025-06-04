using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using xhunter74.CollectionManager.API.Permissions;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Features.Authorize;

public class AuthorizeUtils: ControllerBase
{
    public async Task<Microsoft.AspNetCore.Mvc.SignInResult> CreateTokenForUserAsync(UserManager<ApplicationUser> userManager, 
        ApplicationUser user)
    {
        var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);

        identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString(), OpenIddictConstants.Destinations.AccessToken);
        identity.AddClaim(OpenIddictConstants.Claims.Username, user.UserName, OpenIddictConstants.Destinations.AccessToken);

        var userClaims = await userManager.GetClaimsAsync(user);

        foreach (var claim in userClaims.Where(e => e.Type == AppClaimTypes.UserPermissionClaim))
        {
            claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);
            identity.AddClaim(claim);
        }

        var claimsPrincipal = new ClaimsPrincipal(identity);
        claimsPrincipal.SetScopes(new string[]
        {
                OpenIddictConstants.Scopes.Roles,
                OpenIddictConstants.Scopes.OfflineAccess,
                OpenIddictConstants.Scopes.Email,
                OpenIddictConstants.Scopes.Profile,
        });

        return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}
