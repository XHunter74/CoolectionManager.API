using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Exceptions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace xhunter74.CollectionManager.API.Features.Items;

public class UpdateItemCommand : ICommand<ItemDto>
{
    public Guid UserId { get; set; }
    public Guid ItemId { get; set; }
    public CreateItemDto Model { get; set; }
}

public class UpdateItemCommandHandler : ICommandHandler<UpdateItemCommand, ItemDto>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly IMongoDbContext _mongoDbContext;
    private readonly ILogger<UpdateItemCommandHandler> _logger;

    public UpdateItemCommandHandler(
        CollectionsDbContext dbContext,
        IMongoDbContext mongoDbContext,
        ILogger<UpdateItemCommandHandler> logger)
    {
        _dbContext = dbContext;
        _mongoDbContext = mongoDbContext;
        _logger = logger;
    }

    public async Task<ItemDto> HandleAsync(UpdateItemCommand command, CancellationToken cancellationToken)
    {
        var itemInDb = await _mongoDbContext.CollectionItems
            .GetByIdAsync(command.ItemId, cancellationToken);

        if (itemInDb == null)
        {
            _logger.LogWarning("Item with ID {Id} not found for user {UserId}", command.ItemId, command.UserId);
            throw new NotFoundException($"Item '{command.ItemId}' not found");
        }

        var collection = await _dbContext.Collections
             .Include(c => c.Fields)
             .AsNoTracking()
             .AsSplitQuery()
             .FirstOrDefaultAsync(c => c.Id == itemInDb.CollectionId && c.OwnerId == command.UserId, cancellationToken);

        if (collection == null)
        {
            _logger.LogWarning("Collection with ID {Id} not found for user {UserId}", itemInDb.CollectionId, command.UserId);
            throw new NotFoundException($"Collection '{itemInDb.CollectionId}' not found");
        }

        var itemDoc = new CollectionItemRecord
        {
            Id = itemInDb.Id,
            CollectionId = itemInDb.CollectionId,
            Created = itemInDb.Created,
            Updated = DateTime.UtcNow
        };

        itemDoc.Fields.Add(Constants.DisplayNameFieldName, (BsonValue)command.Model.DisplayName);
        itemDoc.Fields.Add(Constants.PictureFieldName, (BsonValue)(command.Model.Picture != null ?
            command.Model.Picture.ToString()
            : BsonNull.Value));


        string[] excludedFields = { Constants.DisplayNameFieldName, Constants.PictureFieldName };

        foreach (var field in collection.Fields.Where(e => !excludedFields.Contains(e.DisplayName)))
        {
            var item = command.Model.Values.FirstOrDefault(i => i.FieldId.Equals(field.Id));

            if (item == null && field.IsRequired)
            {
                _logger.LogWarning("Required field {FieldName} is missing in collection {CollectionId} for user {UserId}",
                    field.DisplayName, itemInDb.CollectionId, command.UserId);
                throw new BadRequestException($"Required field '{field.DisplayName}' is missing");
            }
            if (item != null)
            {
                itemDoc.Fields.Add(field.Id.ToString(), item.Value != null ? item.Value : BsonNull.Value);
            }
        }

        _ = await _mongoDbContext.CollectionItems.UpdateAsync(itemInDb.Id, itemDoc, cancellationToken);

        var updatedItem = await _mongoDbContext.CollectionItems
            .GetByIdAsync(command.ItemId, cancellationToken);

        var resultItem = ItemUtils.ConvertMongoItemToItemDto(collection.Fields, updatedItem);

        return resultItem;
    }
}
