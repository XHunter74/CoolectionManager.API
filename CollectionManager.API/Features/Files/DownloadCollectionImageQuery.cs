using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;
using xhunter74.CollectionManager.Shared.Services.Interfaces;

namespace xhunter74.CollectionManager.API.Features.Files;

public class DownloadCollectionImageQuery : IQuery<(string fileName, byte[] sources)>
{
    public Guid UserId { get; set; }
    public Guid ImageId { get; set; }
    public Guid CollectionId { get; set; }
}

public class DownloadCollectionImageQueryHandler : BaseFeatureHandler,
    IQueryHandler<DownloadCollectionImageQuery, (string fileName, byte[] sources)>
{
    private readonly IStorageService _storageService;
    private readonly CollectionsDbContext _dbContext;
    private readonly ILogger<DownloadUserImageQueryHandler> _logger;

    public DownloadCollectionImageQueryHandler(
        ILogger<DownloadUserImageQueryHandler> logger,
        IStorageService storageService,
        CollectionsDbContext dbContext
        ) : base(logger)
    {
        _storageService = storageService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<(string fileName, byte[] sources)> HandleAsync(DownloadCollectionImageQuery query, CancellationToken cancellationToken)
    {
        var collection = await _dbContext.Collections
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.CollectionId && c.OwnerId == query.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", query.CollectionId, query.UserId);
            throw new NotFoundException("Collection does not exist with this Id.");
        }

        var file = await _dbContext.Files
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == query.ImageId, cancellationToken);

        if (file == null)
        {
            _logger.LogWarning("File with ID {Id} not found for user {UserId}", query.ImageId, query.UserId);
            throw new NotFoundException("File does not exist with this Id.");
        }

        var sources = await _storageService.GetFileAsync(query.UserId, query.ImageId, cancellationToken);

        if (sources == null || sources.Length == 0)
        {
            _logger.LogWarning("File with ID {Id} has no sources for user {UserId}", query.ImageId, query.UserId);
            throw new NotFoundException("File does not have any sources.");
        }

        return (file.Name, sources);
    }
}
