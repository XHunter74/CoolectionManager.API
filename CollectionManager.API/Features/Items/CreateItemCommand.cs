using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Items;

public class CreateItemCommand : ICommand<DynamicItemRecord>
{
    public Guid CollectionId { get; set; }
    public Guid UserId { get; set; }
    public CreateItemDto[] Model { get; set; }
}

public class CreateItemCommandHandler : ICommandHandler<CreateItemCommand, DynamicItemRecord>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly IMongoDbContext _mongoDbContext;
    private readonly ILogger<CreateItemCommandHandler> _logger;

    public CreateItemCommandHandler(CollectionsDbContext dbContext, IMongoDbContext mongoDbContext, ILogger<CreateItemCommandHandler> logger)
    {
        _dbContext = dbContext;
        _mongoDbContext = mongoDbContext;
        _logger = logger;
    }

    public async Task<DynamicItemRecord> HandleAsync(CreateItemCommand command, CancellationToken cancellationToken)
    {
        var collection = await _dbContext.Collections
            .Include(c => c.Fields)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == command.CollectionId && c.OwnerId == command.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", command.CollectionId, command.UserId);
            throw new NotFoundException($"Collection '{command.CollectionId}' not found");
        }

        var itemDoc = new DynamicItemRecord { CollectionId = command.CollectionId };

        foreach (var item in command.Model)
        {
            var field = collection.Fields
                .FirstOrDefault(f => f.Name.Equals(item.Name, StringComparison.InvariantCultureIgnoreCase));

            if (field == null)
            {
                _logger.LogWarning("Field with name {FieldName} not found in collection {CollectionId} for user {UserId}", item.Name, command.CollectionId, command.UserId);
                throw new BadRequestException($"Field '{item.Name}' not found in collection");
            }

            itemDoc.Fields.Add(field.Name, item.Value != null ? (BsonValue)item.Value : BsonNull.Value);
        }
        var newItem = await _mongoDbContext.CollectionItems.AddAsync(itemDoc, cancellationToken);

        return newItem;
    }
}
