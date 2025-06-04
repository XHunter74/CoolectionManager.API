using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class DeleteCollectionFieldCommand : ICommand<bool>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}

public class DeleteCollectionFieldCommandHandler : ICommandHandler<DeleteCollectionFieldCommand, bool>
{
    private readonly ILogger<DeleteCollectionFieldCommandHandler> _logger;
    private readonly CollectionsDbContext _dbContext;

    public DeleteCollectionFieldCommandHandler(
        ILogger<DeleteCollectionFieldCommandHandler> logger, CollectionsDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<bool> HandleAsync(DeleteCollectionFieldCommand command, CancellationToken cancellationToken)
    {
        var field = await _dbContext.CollectionFields
            .FirstOrDefaultAsync(f => f.Id == command.Id && f.Collection.OwnerId == command.UserId, cancellationToken);

        if (field == null)
        {
            _logger.LogWarning("Field with ID {Id} not found for user {UserId}", command.Id, command.UserId);
            throw new NotFoundException($"Field with ID {command.Id} not found");
        }

        _dbContext.CollectionFields.Remove(field);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
