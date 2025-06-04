using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Identity;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Authorize;

public class GetTokenForRefreshTokenCommand : ICommand<Microsoft.AspNetCore.Mvc.SignInResult>
{
    public string UserId { get; set; }
}

public class GetTokenForRefreshTokenCommandHandler : BaseFeatureHandler, ICommandHandler<GetTokenForRefreshTokenCommand,
    Microsoft.AspNetCore.Mvc.SignInResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly CollectionsDbContext _dbContext;

    public GetTokenForRefreshTokenCommandHandler(
        ILogger<GetTokenForPasswordCommandHandler> logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager
        ) : base(logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Microsoft.AspNetCore.Mvc.SignInResult> HandleAsync(GetTokenForRefreshTokenCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(command.UserId))
            throw new BadRequestException("The token is missing a subject claim.");

        var user = await _userManager.FindByIdAsync(command.UserId);
        if (user == null)
            throw new BadRequestException("The refresh token is no longer valid.");


        if (!await _signInManager.CanSignInAsync(user))
            throw new BadRequestException("The user is no longer allowed to sign in.");

        var authorizeUtils = new AuthorizeUtils();
        var result = await authorizeUtils.CreateTokenForUserAsync(_userManager, user);

        return result;
    }
}
