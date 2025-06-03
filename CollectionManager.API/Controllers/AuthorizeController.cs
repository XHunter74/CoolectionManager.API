using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using xhunter74.CollectionManager.API.Data;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
public class AuthorizationController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthorizationController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpPost("~/connect/token"), IgnoreAntiforgeryToken]
    [Consumes("application/x-www-form-urlencoded")]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var oidcRequest = HttpContext.GetOpenIddictServerRequest();
        if (oidcRequest.IsPasswordGrantType())
            return await TokensForPasswordGrantType(oidcRequest);

        if (oidcRequest.IsRefreshTokenGrantType())
        {
            // return tokens for refresh token flow
        }

        if (oidcRequest.GrantType == "custom_flow_name")
        {
            // return tokens for custom flow
        }

        return BadRequest(new OpenIddictResponse
        {
            Error = OpenIddictConstants.Errors.UnsupportedGrantType
        });
    }

    private async Task<IActionResult> TokensForPasswordGrantType(OpenIddictRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
            return Unauthorized();

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (signInResult.Succeeded)
        {
            var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);

            identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString(), OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim(OpenIddictConstants.Claims.Username, user.UserName, OpenIddictConstants.Destinations.AccessToken);

            //foreach (var userRole in user.UserRoles)
            //{
            //    identity.AddClaim(OpenIddictConstants.Claims.Role, userRole.Role.NormalizedName, OpenIddictConstants.Destinations.AccessToken);
            //}

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
        else
            return Unauthorized();
    }
}