using CQRSMediatr.Interfaces;
using xhunter74.CollectionManager.Data.Entity;
using Microsoft.AspNetCore.Identity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Authorize;

public class GetTokenForPasswordCommand : ICommand<Microsoft.AspNetCore.Mvc.SignInResult>
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class GetTokenForPasswordCommandHandler : BaseFeatureHandler, ICommandHandler<GetTokenForPasswordCommand,
    Microsoft.AspNetCore.Mvc.SignInResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly CollectionsDbContext _dbContext;

    public GetTokenForPasswordCommandHandler(
        ILogger<GetTokenForPasswordCommandHandler> logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager
        ) : base(logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Microsoft.AspNetCore.Mvc.SignInResult> HandleAsync(GetTokenForPasswordCommand command, CancellationToken cancellationToken)
    {

        var user = await _userManager.FindByNameAsync(command.Username);
        if (user == null)
            throw new UnauthorizedException();

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, command.Password, false);
        if (signInResult.Succeeded)
        {
            var authorizeUtils = new AuthorizeUtils();
            var result = await authorizeUtils.CreateTokenForUserAsync(_userManager, user);
            return result;
        }
        else
            throw new UnauthorizedException();
    }

}
