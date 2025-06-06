using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using xhunter74.CollectionManager.API.Features.Items;
using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo;
using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Data.Mongo.Repositories;
using xhunter74.CollectionManager.Shared.Exceptions;

namespace CollectionManager.API.Test.Features.Items;

public class CreateItemCommandTests
{
    [Fact(DisplayName = "HandleAsync throws NotFoundException if collection does not exist")]
    public async Task HandleAsync_ThrowsNotFound_IfCollectionMissing()
    {
        var dbContext = new Mock<CollectionsDbContext>();
        var mongoDbContext = new Mock<IMongoDbContext>();
        var logger = new Mock<ILogger<CreateItemCommandHandler>>();
        dbContext.Setup(x => x.Collections
            .Include(It.IsAny<string>())
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Collection, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Collection)null);
        var handler = new CreateItemCommandHandler(dbContext.Object, mongoDbContext.Object, logger.Object);
        var command = new CreateItemCommand { CollectionId = Guid.NewGuid(), UserId = Guid.NewGuid(), Model = Array.Empty<CreateItemDto>() };
        await Assert.ThrowsAsync<NotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact(DisplayName = "HandleAsync throws BadRequestException if required field is missing")]
    public async Task HandleAsync_ThrowsBadRequest_IfRequiredFieldMissing()
    {
        var collection = new Collection
        {
            Id = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Fields = new List<CollectionField> { new CollectionField { Name = "Field1", IsRequired = true } }
        };
        var dbContext = new Mock<CollectionsDbContext>();
        var mongoDbContext = new Mock<IMongoDbContext>();
        var logger = new Mock<ILogger<CreateItemCommandHandler>>();
        dbContext.Setup(x => x.Collections
            .Include(It.IsAny<string>())
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Collection, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(collection);
        var handler = new CreateItemCommandHandler(dbContext.Object, mongoDbContext.Object, logger.Object);
        var command = new CreateItemCommand { CollectionId = collection.Id, UserId = collection.OwnerId, Model = Array.Empty<CreateItemDto>() };
        await Assert.ThrowsAsync<BadRequestException>(() => handler.HandleAsync(command, CancellationToken.None));
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

        var contextOptions = new DbContextOptionsBuilder<CollectionsDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;

        using var dbContext = new CollectionsDbContext(contextOptions);
        dbContext.Collections.Add(collection);
        dbContext.SaveChanges();

        var logger = new Mock<ILogger<CreateItemCommandHandler>>();

        var itemRecord = new DynamicItemRecord { CollectionId = collection.Id };

        var mongoDbContext = new FakeMongoDbContext(itemRecord);

        var handler = new CreateItemCommandHandler(dbContext, mongoDbContext, logger.Object);

        var command = new CreateItemCommand
        {
            CollectionId = collection.Id,
            UserId = collection.OwnerId,
            Model = new[] { new CreateItemDto { Name = "Field1", Value = "val" } }
        };
       
        var result = await handler.HandleAsync(command, CancellationToken.None);
        
        Assert.NotNull(result);
        var dict = (IDictionary<string, object>)result;
        Assert.Equal(itemRecord.CollectionId, dict["CollectionId"]);
    }

    private class FakeMongoDbContext : IMongoDbContext
    {
        private readonly DynamicItemRecord _item;
        public FakeMongoDbContext(DynamicItemRecord item)
        {
            _item = item;
            // Mock IMongoDatabase and IClientSessionHandle
            var mongoDatabaseMock = new Mock<MongoDB.Driver.IMongoDatabase>();
            var sessionMock = new Mock<MongoDB.Driver.IClientSessionHandle>();
            CollectionItems = new FakeCollectionsRepository(item, mongoDatabaseMock.Object, sessionMock.Object);
        }

        public CollectionsRepository CollectionItems { get; }
        public Task CommitAsync() => Task.CompletedTask;
        public void Dispose() { }

        private class FakeCollectionsRepository : CollectionsRepository
        {
            private readonly DynamicItemRecord _item;
            public FakeCollectionsRepository(DynamicItemRecord item, MongoDB.Driver.IMongoDatabase db, MongoDB.Driver.IClientSessionHandle session)
                : base(db, session, "Fake") { _item = item; }
            public override Task<DynamicItemRecord> AddAsync(DynamicItemRecord entity, CancellationToken cancellationToken) => Task.FromResult(_item);
        }
    }

}
