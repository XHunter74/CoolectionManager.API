using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Identity;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Users;

public class ResetPasswordCommand : ICommand<bool>
{
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
}

public class ResetPasswordCommandHandler : BaseFeatureHandler, ICommandHandler<ResetPasswordCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetPasswordCommandHandler(
        ILogger<CreateUserHandler> logger,
        UserManager<ApplicationUser> userManager
        ) : base(logger)
    {
        _userManager = userManager;
    }

    public async Task<bool> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user == null)
        {
            throw new NotFoundException("User does not exist with this ID.");
        }

        var result = await _userManager.ResetPasswordAsync(user, command.Token, command.NewPassword);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            Logger.LogError("Password reset failed for user {UserId}: {ErrorMessage}", command.UserId, errorMessage);
            throw new BadRequestException($"Password reset failed: {errorMessage}");
        }

        return true;
    }

}
