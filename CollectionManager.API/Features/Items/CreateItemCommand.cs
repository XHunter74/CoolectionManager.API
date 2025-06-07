using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using System.Dynamic;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo;
using xhunter74.CollectionManager.Data.Mongo.Extensions;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Items;

public class CreateItemCommand : ICommand<ExpandoObject>
{
    public Guid CollectionId { get; set; }
    public Guid UserId { get; set; }
    public CreateItemDto[] Model { get; set; }
}

public class CreateItemCommandHandler : ICommandHandler<CreateItemCommand, ExpandoObject>
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

    public async Task<ExpandoObject> HandleAsync(CreateItemCommand command, CancellationToken cancellationToken)
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

        var itemDoc = new CollectionItemRecord { CollectionId = command.CollectionId };

        foreach (var field in collection.Fields)
        {
            var item = command.Model.FirstOrDefault(i => i.Name.Equals(field.Name, StringComparison.InvariantCultureIgnoreCase));

            if (item == null && field.IsRequired)
            {
                _logger.LogWarning("Required field {FieldName} is missing in collection {CollectionId} for user {UserId}", field.Name, command.CollectionId, command.UserId);
                throw new BadRequestException($"Required field '{field.Name}' is missing");
            }
            itemDoc.Fields.Add(field.Name, item.Value != null ? (BsonValue)item.Value : BsonNull.Value);
        }

        var newItem = await _mongoDbContext.CollectionItems.AddAsync(itemDoc, cancellationToken);

        return newItem.ToFlattenedExpando();
    }
}
