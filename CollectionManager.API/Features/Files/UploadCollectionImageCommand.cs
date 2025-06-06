using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;
using xhunter74.CollectionManager.Shared.Services.Interfaces;
using File = xhunter74.CollectionManager.Data.Entity.File;

namespace xhunter74.CollectionManager.API.Features.Files;

public class UploadCollectionImageCommand : ICommand<bool>
{
    public Guid UserId { get; init; }
    public Guid CollectionId { get; init; }
    public string FileName { get; init; }
    public byte[] Sources { get; set; }
}

public class UploadCollectionImageCommandHandler : BaseFeatureHandler,
    ICommandHandler<UploadCollectionImageCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStorageService _storageService;
    private readonly IImageService _imageService;
    private readonly CollectionsDbContext _dbContext;

    public UploadCollectionImageCommandHandler(
        ILogger<UploadCollectionImageCommandHandler> logger,
       CollectionsDbContext dbContext,
        IStorageService storageService,
        IImageService imageService
        ) : base(logger)
    {
        _storageService = storageService;
        _imageService = imageService;
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(UploadCollectionImageCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var collection = await _dbContext.Collections
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == command.CollectionId &&
                    c.OwnerId == command.UserId, cancellationToken);
            if (collection == null)
            {
                Logger.LogWarning("Collection with ID {CollectionId} not found for user {UserId}", command.CollectionId, command.UserId);
                throw new NotFoundException("Collection not found.");
            }

            var file = new File
            {
                Id = Guid.NewGuid(),
                Name = command.FileName,
                CollectionId = collection.Id
            };

            var pngSources = await _imageService.ConvertToPngAsync(command.Sources);

            try
            {
                await _storageService.UploadFileAsync(command.UserId, file.Id, pngSources, cancellationToken);
                await _dbContext.Files.AddAsync(file, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error uploading file {FileName} for user {UserId}", command.FileName, command.UserId);
                throw new AppErrorException("Failed to upload file.");
            }
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error uploading avatar for user {UserId}", command.UserId);
            throw new AppErrorException("Failed to upload avatar.");
        }
    }
}
