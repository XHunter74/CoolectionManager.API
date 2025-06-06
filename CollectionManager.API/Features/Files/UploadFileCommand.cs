using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;
using xhunter74.CollectionManager.Shared.Services.Interfaces;
using File = xhunter74.CollectionManager.Data.Entity.File;

namespace xhunter74.CollectionManager.API.Features.Files;

public class UploadFileCommand : ICommand<Guid>
{
    public Guid CollectionId { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; }
    public byte[] Sources { get; set; }

}

public class UploadFileCommandHandler : ICommandHandler<UploadFileCommand, Guid>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly ILogger<UploadFileCommandHandler> _logger;

    public UploadFileCommandHandler(
        CollectionsDbContext dbContext,
        IStorageService storageService,
        ILogger<UploadFileCommandHandler> logger)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<Guid> HandleAsync(UploadFileCommand command, CancellationToken cancellationToken)
    {
        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(c => c.Id == command.CollectionId && c.OwnerId == command.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", command.CollectionId, command.UserId);
            throw new NotFoundException($"Collection with ID {command.CollectionId} not found");
        }

        var newFile = new File
        {
            Id = Guid.NewGuid(),
            Name = command.FileName,
            CollectionId = command.CollectionId,
        };

        try
        {
            await _storageService.UploadFileAsync(command.UserId, newFile.Id, command.Sources, cancellationToken);
            _dbContext.Files.Add(newFile);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName} for user {UserId}", command.FileName, command.UserId);
            throw new BadRequestException($"Failed to upload file: {ex.Message}");
        }

        return newFile.Id;
    }
}
