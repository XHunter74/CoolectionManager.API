using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace xhunter74.CollectionManager.API.Features.Items;

public class CreateItemCommand : ICommand<ItemDto>
{
    public Guid CollectionId { get; set; }
    public Guid UserId { get; set; }
    public CreateItemDto Model { get; set; }
}

public class CreateItemCommandHandler : ICommandHandler<CreateItemCommand, ItemDto>
{
    private readonly CollectionsDbContext _dbContext;
    private readonly IMongoDbContext _mongoDbContext;
    private readonly ILogger<CreateItemCommandHandler> _logger;

    public CreateItemCommandHandler(
        CollectionsDbContext dbContext,
        IMongoDbContext mongoDbContext,
        ILogger<CreateItemCommandHandler> logger
        )
    {
        _dbContext = dbContext;
        _mongoDbContext = mongoDbContext;
        _logger = logger;
    }

    public async Task<ItemDto> HandleAsync(CreateItemCommand command, CancellationToken cancellationToken)
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

        var itemDoc = new CollectionItemRecord
        {
            CollectionId = command.CollectionId,
            Created = DateTime.UtcNow,
            Updated = DateTime.UtcNow
        };

        itemDoc.Fields.Add(Constants.DisplayNameFieldName, (BsonValue)command.Model.DisplayName);
        itemDoc.Fields.Add(Constants.PictureFieldName, (BsonValue)(command.Model.Picture != null ?
            command.Model.Picture.ToString()
            : BsonNull.Value));


        string[] excludedFields = { Constants.DisplayNameFieldName, Constants.PictureFieldName };

        foreach (var field in collection.Fields.Where(e => !excludedFields.Contains(e.Name)))
        {
            var item = command.Model.Values.FirstOrDefault(i => i.FieldId.Equals(field.Id));

            if (item == null && field.IsRequired)
            {
                _logger.LogWarning("Required field {FieldName} is missing in collection {CollectionId} for user {UserId}", field.Name, command.CollectionId, command.UserId);
                throw new BadRequestException($"Required field '{field.Name}' is missing");
            }
            if (item != null)
            {
                itemDoc.Fields.Add(field.Id.ToString(), item.Value != null ? item.Value : BsonNull.Value);
            }
        }

        var newItem = await _mongoDbContext.CollectionItems.AddAsync(itemDoc, cancellationToken);

        var resultItem = ItemUtils.ConvertMongoItemToItemDto(collection.Fields, newItem);

        return resultItem;
    }
}
