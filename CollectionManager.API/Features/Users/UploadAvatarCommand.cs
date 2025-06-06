using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Identity;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;
using xhunter74.CollectionManager.Shared.Services.Interfaces;

namespace xhunter74.CollectionManager.API.Features.Users;

public class UploadAvatarCommand : ICommand<bool>
{
    public Guid UserId { get; init; }
    public byte[] Sources { get; set; }
}

public class UploadAvatarCommandHandler : BaseFeatureHandler, ICommandHandler<UploadAvatarCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStorageService _storageService;
    private readonly IImageService _imageService;

    public UploadAvatarCommandHandler(
        ILogger<UploadAvatarCommandHandler> logger,
        UserManager<ApplicationUser> userManager,
        IStorageService storageService,
        IImageService imageService
        ) : base(logger)
    {
        _userManager = userManager;
        _storageService = storageService;
        _imageService = imageService;
    }

    public async Task<bool> HandleAsync(UploadAvatarCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());
            var fileId = user!.Avatar.HasValue ? user.Avatar.Value : Guid.NewGuid();

            var pngSources = await _imageService.ConvertToPngAsync(command.Sources);

            await _storageService.UploadFileAsync(user.Id, fileId, pngSources, cancellationToken);

            user.Avatar = fileId;
            var result = await _userManager.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error uploading avatar for user {UserId}", command.UserId);
            throw new AppErrorException("Failed to upload avatar.");
        }
    }
}
