using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class CreateCollectionFieldCommand : ICommand<CollectionFieldDto>
{
    public Guid CollectionId { get; set; }
    public Guid UserId { get; set; }
    public CreateCollectionFieldDto Model { get; set; }
}

public class CreateCollectionFieldCommandHandler : ICommandHandler<CreateCollectionFieldCommand, CollectionFieldDto>
{
    private readonly ILogger<CreateCollectionFieldCommandHandler> _logger;
    private readonly CollectionsDbContext _dbContext;

    public CreateCollectionFieldCommandHandler(
        ILogger<CreateCollectionFieldCommandHandler> logger,
        CollectionsDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CollectionFieldDto> HandleAsync(CreateCollectionFieldCommand command, CancellationToken cancellationToken)
    {
        var collection = await _dbContext.Collections
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == command.CollectionId && c.OwnerId == command.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", command.CollectionId, command.UserId);
            throw new NotFoundException($"Collection with ID {command.CollectionId} not found");
        }

        var field = new CollectionField
        {
            Id = Guid.NewGuid(),
            Name = command.Model.DisplayName,
            Type = command.Model.Type,
            Order = command.Model.Order,
            CollectionId = command.CollectionId
        };

        _dbContext.CollectionFields.Add(field);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CollectionFieldDto
        {
            Id = field.Id,
            DisplayName = field.Name,
            Type = field.Type,
            IsRequired = field.IsRequired,
            Order = field.Order,
            CollectionId = field.CollectionId
        };
    }
}