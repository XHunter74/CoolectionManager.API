using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.CollectionFields;

public class ChangeCollectionFieldOrderCommand : ICommand<CollectionFieldDto>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Order { get; set; }
}

public class ChangeCollectionFieldOrderCommandHandler : ICommandHandler<ChangeCollectionFieldOrderCommand, CollectionFieldDto>
{
    private readonly ILogger<ChangeCollectionFieldOrderCommandHandler> _logger;
    private readonly CollectionsDbContext _dbContext;

    public ChangeCollectionFieldOrderCommandHandler(
        ILogger<ChangeCollectionFieldOrderCommandHandler> logger,
        CollectionsDbContext dbContext
        )
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CollectionFieldDto> HandleAsync(ChangeCollectionFieldOrderCommand command, 
        CancellationToken cancellationToken)
    {
        var field = await _dbContext.CollectionFields
            .FirstOrDefaultAsync(f => f.Id == command.Id && f.Collection.OwnerId == command.UserId, cancellationToken);

        if (field == null)
        {
            _logger.LogWarning("Field with ID {Id} not found or not owned by user {UserId}", command.Id, command.UserId);
            throw new NotFoundException("Field not found or not owned by user.");
        }

        field.Order = command.Order;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CollectionFieldDto
        {
            Id = field.Id,
            DisplayName = field.DisplayName,
            Type = field.Type,
            IsRequired = field.IsRequired,
            Order = field.Order
        };
    }
}
