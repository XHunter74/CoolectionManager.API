using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class UpdateCollectionFieldCommand : ICommand<CollectionFieldDto>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UpdateCollectionFieldDto Model { get; set; }
}

public class UpdateCollectionFieldCommandHandler : ICommandHandler<UpdateCollectionFieldCommand, CollectionFieldDto>
{
    private readonly ILogger<UpdateCollectionFieldCommandHandler> _logger;
    private readonly CollectionsDbContext _dbContext;

    public UpdateCollectionFieldCommandHandler(
        ILogger<UpdateCollectionFieldCommandHandler> logger,
        CollectionsDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CollectionFieldDto> HandleAsync(UpdateCollectionFieldCommand command, CancellationToken cancellationToken)
    {
        var field = await _dbContext.CollectionFields
            .FirstOrDefaultAsync(f => f.Id == command.Id && f.Collection.OwnerId == command.UserId, cancellationToken);

        if (field == null)
        {
            _logger.LogWarning("Collection field with ID {Id} not found for user {UserId}", command.Id, command.UserId);
            throw new NotFoundException($"Collection field with ID {command.Id} not found");
        }

        field.DisplayName = command.Model.DisplayName;
        field.Type = command.Model.Type;
        field.IsRequired = command.Model.IsRequired;
        field.Order = command.Model.Order;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CollectionFieldDto
        {
            Id = field.Id,
            DisplayName = field.DisplayName,
            IsSystem = field.IsSystem,
            Type = field.Type,
            IsRequired = field.IsRequired,
            Order = field.Order,
            CollectionId = field.CollectionId
        };
    }
}