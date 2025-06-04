using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Collections;

public class DeleteCollectionCommand : ICommand<bool>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}

public class DeleteCollectionCommandHandler : ICommandHandler<DeleteCollectionCommand, bool>
{
    private readonly ILogger<DeleteCollectionCommandHandler> _logger;
    private readonly CollectionsDbContext _dbContext;

    public DeleteCollectionCommandHandler(ILogger<DeleteCollectionCommandHandler> logger, CollectionsDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(DeleteCollectionCommand command, CancellationToken cancellationToken)
    {
        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.OwnerId == command.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", command.Id, command.UserId);
            throw new NotFoundException($"Collection with ID {command.Id} not found");
        }
        _dbContext.Collections.Remove(collection);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
