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

    [Fact(DisplayName = "HandleAsync throws NotFoundException if item does not exist")]
    public async Task HandleAsync_ThrowsNotFound_IfItemMissing()
    {
        var query = new GetItemByIdQuery { ItemId = Guid.NewGuid(), UserId = Guid.NewGuid() };
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact(DisplayName = "HandleAsync throws NotFoundException if collection does not exist or not owned by user")]
    public async Task HandleAsync_ThrowsNotFound_IfCollectionMissingOrNotOwned()
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

    [Fact(DisplayName = "HandleAsync returns flattened expando when item and collection exist and user is owner")]
    public async Task HandleAsync_ReturnsExpando_WhenSuccess()
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
        var dict = (IDictionary<string, object>)result;
        Assert.Equal(itemId, dict["Id"]);
        Assert.Equal(collectionId, dict["CollectionId"]);
    }
}
