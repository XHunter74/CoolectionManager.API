using xhunter74.CollectionManager.API.Features.Items;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace CollectionManager.API.Test.Features.Items;

public class GetItemsQueryTests : BaseConnectorTest<GetItemsQueryHandler>
{
    private readonly GetItemsQueryHandler _handler;

    public GetItemsQueryTests()
    {
        _handler = new GetItemsQueryHandler(CollectionsDbContext, MongoDbContextMock, LoggerMock.Object);
    }

    [Fact(DisplayName = "HandleAsync throws NotFoundException when collection does not exist or not owned by user")]
    public async Task HandleAsync_ThrowsNotFoundException_WhenCollectionDoesNotExistOrNotOwnedByUser()
    {
        var collectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var query = new GetItemsQuery { CollectionId = collectionId, UserId = userId };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact(DisplayName = "HandleAsync returns empty list when collection exists but no items")]
    public async Task HandleAsync_ReturnsEmptyList_WhenCollectionExistsButNoItems()
    {
        var collectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

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

        var query = new GetItemsQuery { CollectionId = collectionId, UserId = userId };

        var result = (await _handler.HandleAsync(query, CancellationToken.None)).ToList();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "HandleAsync returns ItemDto list when items exist and user is owner")]
    public async Task HandleAsync_ReturnsItemDtoList_WhenItemsExistAndUserIsOwner()
    {
        var collectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var item1 = new CollectionItemRecord { Id = Guid.NewGuid(), CollectionId = collectionId };
        var item2 = new CollectionItemRecord { Id = Guid.NewGuid(), CollectionId = collectionId };

        ((FakeMongoDbContext)MongoDbContextMock).AddItem(item1);
        ((FakeMongoDbContext)MongoDbContextMock).AddItem(item2);

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

        var query = new GetItemsQuery { CollectionId = collectionId, UserId = userId };
        
        var result = (await _handler.HandleAsync(query, CancellationToken.None)).ToList();
        
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == item1.Id && r.CollectionId == collectionId);
        Assert.Contains(result, r => r.Id == item2.Id && r.CollectionId == collectionId);
    }
}
