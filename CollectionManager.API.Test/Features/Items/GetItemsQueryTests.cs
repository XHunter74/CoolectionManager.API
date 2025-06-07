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

    [Fact(DisplayName = "HandleAsync throws NotFoundException if collection does not exist or not owned by user")]
    public async Task HandleAsync_ThrowsNotFound_IfCollectionMissingOrNotOwned()
    {
        var collectionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var query = new GetItemsQuery { CollectionId = collectionId, UserId = userId };

        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact(DisplayName = "HandleAsync returns empty list if collection exists but no items")]
    public async Task HandleAsync_ReturnsEmptyList_IfNoItems()
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

        var result = await _handler.HandleAsync(query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact(DisplayName = "HandleAsync returns flattened expando list when items exist and user is owner")]
    public async Task HandleAsync_ReturnsExpandoList_WhenSuccess()
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
        var dict1 = (IDictionary<string, object>)result[0];
        var dict2 = (IDictionary<string, object>)result[1];
        Assert.Contains(dict1["Id"], new object[] { item1.Id, item2.Id });
        Assert.Contains(dict2["Id"], new object[] { item1.Id, item2.Id });
        Assert.Equal(collectionId, dict1["CollectionId"]);
        Assert.Equal(collectionId, dict2["CollectionId"]);
    }
}
