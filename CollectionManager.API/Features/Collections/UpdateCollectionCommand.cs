using CQRSMediatr.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Collections;

public class UpdateCollectionCommand : ICommand<CollectionDto>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UpdateCollectionDto Model { get; set; }
}

public class UpdateCollectionCommandHandler : ICommandHandler<UpdateCollectionCommand, CollectionDto>
{
    private readonly ILogger<UpdateCollectionCommandHandler> _logger;
    private readonly CollectionsDbContext _dbContext;

    public UpdateCollectionCommandHandler(ILogger<UpdateCollectionCommandHandler> logger, CollectionsDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<CollectionDto> HandleAsync(UpdateCollectionCommand command, CancellationToken cancellationToken)
    {
        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.OwnerId == command.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", command.Id, command.UserId);
            throw new NotFoundException($"Collection with ID {command.Id} not found");
        }


        collection.Name = command.Model.Name;
        collection.Description = command.Model.Description;
        collection.Image = command.Model.Image;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CollectionDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Description = collection.Description,
            Image = collection.Image
        };
    }
}
