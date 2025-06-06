using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Services.Interfaces;
using CQRSMediatr.Interfaces;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Files;

public class DownloadFileQuery : IQuery<(string fileName, byte[] sources)>
{
    public Guid FileId { get; set; }
    public Guid UserId { get; set; }
}

public class DownloadFileQueryHandler : IQueryHandler<DownloadFileQuery, (string fileName, byte[] sources)>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly IStorageService _storageService;
    private readonly ILogger<DownloadFileQueryHandler> _logger;

    public DownloadFileQueryHandler(
        CollectionsDbContext dbContext,
        IStorageService storageService,
        ILogger<DownloadFileQueryHandler> logger)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<(string fileName, byte[] sources)> HandleAsync(DownloadFileQuery query, CancellationToken cancellationToken)
    {
        var file = await _dbContext.Files
            .FirstOrDefaultAsync(f => f.Id == query.FileId && f.Collection.OwnerId == query.UserId, cancellationToken);

        if (file == null)
        {
            _logger.LogWarning("File with ID {Id} not found for user {UserId}", query.FileId, query.UserId);
            throw new NotFoundException($"File with ID {query.FileId} not found");
        }

        var fileBytes = await _storageService.GetFileAsync(query.UserId, file.Id, cancellationToken);

        if (fileBytes == null || fileBytes.Length == 0)
        {
            _logger.LogWarning("File content for ID {Id} not found in storage for user {UserId}", query.FileId, query.UserId);
            throw new NotFoundException($"File content not found");
        }
        return (file.Name, fileBytes);
    }
}

