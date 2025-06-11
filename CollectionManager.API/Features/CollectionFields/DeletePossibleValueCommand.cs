using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class DeletePossibleValueCommand : ICommand<bool>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}

public class DeletePossibleValueCommandHandler : ICommandHandler<DeletePossibleValueCommand, bool>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly ILogger<DeletePossibleValueCommandHandler> _logger;
    public DeletePossibleValueCommandHandler(CollectionsDbContext dbContext, ILogger<DeletePossibleValueCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> HandleAsync(DeletePossibleValueCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting possible value {PossibleValueId}", command.Id);

        var possibleValue = await _dbContext.PossibleValues
            .Include(v => v.CollectionField).ThenInclude(v => v.Collection)
            .FirstOrDefaultAsync(v => v.Id == command.Id, cancellationToken);

        if (possibleValue == null || possibleValue.CollectionField.Collection.OwnerId != command.UserId)
        {
            _logger.LogWarning("Possible value {PossibleValueId} not found or not owned by user", command.Id);
            throw new NotFoundException($"Possible value {command.Id} not found or not owned by user");
        }

        _dbContext.PossibleValues.Remove(possibleValue);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted possible value {PossibleValueId}", command.Id);

        return true;
    }
}
