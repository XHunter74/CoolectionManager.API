using xhunter74.CollectionManager.API.Features.Items;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace CollectionManager.API.Test.Features.Items;

public class GetItemByIdQueryTests : BaseConnectorTest<GetItemByIdQueryHandler>
{
    private readonly GetItemByIdQueryHandler _handler;

    public GetItemByIdQueryTests()
    {
        _handler = new GetItemByIdQueryHandler(CollectionsDbContext, MongoDbContextMock, LoggerMock.Object);
    }

    [Fact(DisplayName = "HandleAsync throws NotFoundException when item does not exist")]
    public async Task HandleAsync_ThrowsNotFoundException_WhenItemDoesNotExist()
    {
        var query = new GetItemByIdQuery { ItemId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact(DisplayName = "HandleAsync throws NotFoundException when collection does not exist or not owned by user")]
    public async Task HandleAsync_ThrowsNotFoundException_WhenCollectionDoesNotExistOrNotOwnedByUser()
    {
        var itemId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var item = new CollectionItemRecord { Id = itemId, CollectionId = collectionId };
        ((FakeMongoDbContext)MongoDbContextMock).AddItem(item);
        // No collection added to CollectionsDbContext
        var query = new GetItemByIdQuery { ItemId = itemId, UserId = userId };
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact(DisplayName = "HandleAsync returns ItemDto when item and collection exist and user is owner")]
    public async Task HandleAsync_ReturnsItemDto_WhenItemAndCollectionExistAndUserIsOwner()
    {
        var itemId = Guid.NewGuid();
        var collectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var item = new CollectionItemRecord { Id = itemId, CollectionId = collectionId };
        ((FakeMongoDbContext)MongoDbContextMock).AddItem(item);

        CollectionsDbContext.Collections.Add(
            new Collection
            {
                Id = collectionId,
                Name = "Test Collection",
                Description = "Test Description",
                OwnerId = userId,
                Fields = new List<CollectionField>()
            });
        CollectionsDbContext.SaveChanges();
        var query = new GetItemByIdQuery { ItemId = itemId, UserId = userId };
        var result = await _handler.HandleAsync(query, CancellationToken.None);
        Assert.NotNull(result);
        Assert.Equal(itemId, result.Id);
        Assert.Equal(collectionId, result.CollectionId);
    }
}
