using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Net;
using xhunter74.CollectionManager.API.Settings;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;
using xhunter74.CollectionManager.Shared.Services.Interfaces;

namespace xhunter74.CollectionManager.API.Features.Users;


public class GenerateResetPasswordLinkCommand : ICommand<bool>
{
    public string UserName { get; set; }
}

public class GenerateResetPasswordLinkCommandHandler : BaseFeatureHandler, ICommandHandler<GenerateResetPasswordLinkCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppSettings _appSettings;
    private readonly IEmailService _emailService;

    public GenerateResetPasswordLinkCommandHandler(
        ILogger<CreateUserHandler> logger,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IOptions<AppSettings> options
        ) : base(logger)
    {
        _userManager = userManager;
        _appSettings = options.Value;
        _emailService = emailService;
    }

    public async Task<bool> HandleAsync(GenerateResetPasswordLinkCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(command.UserName);

        if (user == null)
        {
            throw new NotFoundException("User does not exist with this username.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        token = WebUtility.UrlEncode(token);

        var resetPasswordLink = $"{_appSettings.SiteUrl}reset-password?userId={user.Id}&token={token}";

        var body = $"<p>To reset your password, please click the link below:</p>" +
                      $"<p><a href=\"{resetPasswordLink}\">Reset Password</a></p>" +
                      "<p>If you did not request this, please ignore this email.</p>";

        await _emailService.SendEmailAsync(user.UserName!, "Password Reset Request", body);

        return true;
    }
}
