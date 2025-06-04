using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using xhunter74.CollectionManager.API.Features.Authorize;
using xhunter74.CollectionManager.API.Permissions;
using xhunter74.CollectionManager.Data.Entity;

namespace xhunter74.CollectionManager.API.Controllers;

[ApiController]
public class AuthorizationController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICqrsMediatr _mediatr;

    public AuthorizationController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ICqrsMediatr mediatr
        )
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _mediatr = mediatr;
    }

    [HttpPost("~/connect/token"), IgnoreAntiforgeryToken]
    [Consumes("application/x-www-form-urlencoded")]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var oidcRequest = HttpContext.GetOpenIddictServerRequest();
        if (oidcRequest.IsPasswordGrantType())
        {
            var command = new GetTokenForPasswordCommand
            {
                Username = oidcRequest.Username,
                Password = oidcRequest.Password
            };
            var result = await _mediatr.SendAsync(command);
            return result;
        }

        if (oidcRequest.IsRefreshTokenGrantType())
            return await TokensForRefreshTokenGrantType();

        return BadRequest(new OpenIddictResponse
        {
            Error = OpenIddictConstants.Errors.UnsupportedGrantType
        });
    }

    private async Task<Microsoft.AspNetCore.Mvc.SignInResult> CreateTokenForUserAsync(ApplicationUser user)
    {
        var identity = new ClaimsIdentity(
                TokenValidationParameters.DefaultAuthenticationType,
                OpenIddictConstants.Claims.Name,
                OpenIddictConstants.Claims.Role);

        identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString(), OpenIddictConstants.Destinations.AccessToken);
        identity.AddClaim(OpenIddictConstants.Claims.Username, user.UserName, OpenIddictConstants.Destinations.AccessToken);

        var userClaims = await _userManager.GetClaimsAsync(user);

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

    private async Task<IActionResult> TokensForRefreshTokenGrantType()
    {
        var authResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (authResult?.Principal == null)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "The refresh token is no longer valid."
            });
        }

        var userId = authResult.Principal.GetClaim(OpenIddictConstants.Claims.Subject);
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.ServerError,
                ErrorDescription = "The token is missing a subject claim."
            });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "The refresh token is no longer valid."
            });
        }

        if (!await _signInManager.CanSignInAsync(user))
        {
            return BadRequest(new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "The user is no longer allowed to sign in."
            });
        }

        var result = await CreateTokenForUserAsync(user);

        return result;
    }
}