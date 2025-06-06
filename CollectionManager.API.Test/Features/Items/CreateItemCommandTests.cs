using xhunter74.CollectionManager.API.Features.Items;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace CollectionManager.API.Test.Features.Items;

public class CreateItemCommandTests : BaseConnectorTest<CreateItemCommandHandler>
{
    private readonly CreateItemCommandHandler _handler;

    public CreateItemCommandTests()
    {
        _handler = new CreateItemCommandHandler(CollectionsDbContext, MongoDbContextMock, LoggerMock.Object);
    }


    [Fact(DisplayName = "HandleAsync throws NotFoundException if collection does not exist")]
    public async Task HandleAsync_ThrowsNotFound_IfCollectionMissing()
    {
        var command = new CreateItemCommand
        {
            CollectionId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Model = Array.Empty<CreateItemDto>()
        };
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact(DisplayName = "HandleAsync throws BadRequestException if required field is missing")]
    public async Task HandleAsync_ThrowsBadRequest_IfRequiredFieldMissing()
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Test Collection",
            Description = "Test Collection",
            OwnerId = Guid.NewGuid(),
            Fields = new List<CollectionField> { new CollectionField { Name = "Field1", IsRequired = true } }
        };

        CollectionsDbContext.Collections.Add(collection);
        CollectionsDbContext.SaveChanges();

        var command = new CreateItemCommand
        {
            CollectionId = collection.Id,
            UserId = collection.OwnerId,
            Model = Array.Empty<CreateItemDto>()
        };

        await Assert.ThrowsAsync<BadRequestException>(() => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact(DisplayName = "HandleAsync returns flattened expando when item is created successfully")]
    public async Task HandleAsync_ReturnsExpando_WhenSuccess()
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Name = "Test Collection",
            Description = "Test Collection",
            Fields = new List<CollectionField> { new CollectionField { Name = "Field1", IsRequired = false } }
        };

        CollectionsDbContext.Collections.Add(collection);
        CollectionsDbContext.SaveChanges();

        var itemRecord = new DynamicItemRecord { CollectionId = collection.Id };

        var mongoDbContext = new FakeMongoDbContext();

        var command = new CreateItemCommand
        {
            CollectionId = collection.Id,
            UserId = collection.OwnerId,
            Model = new[] { new CreateItemDto { Name = "Field1", Value = "val" } }
        };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        var dict = (IDictionary<string, object>)result;
        Assert.Equal(itemRecord.CollectionId, dict["CollectionId"]);
    }
}
