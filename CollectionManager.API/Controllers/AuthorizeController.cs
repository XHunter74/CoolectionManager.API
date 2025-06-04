using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using xhunter74.CollectionManager.API.Features.Authorize;
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
            var command = new GetTokenForRefreshTokenCommand
            {
                UserId = userId
            };
            var result = await _mediatr.SendAsync(command);
            return result;
        }

        return BadRequest(new OpenIddictResponse
        {
            Error = OpenIddictConstants.Errors.UnsupportedGrantType
        });
    }
}