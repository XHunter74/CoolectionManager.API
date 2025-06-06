using CQRSMediatr.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo;

namespace CollectionManager.API.Test.Features;

public abstract class BaseConnectorTest<T>
{
    public BaseConnectorTest()
    {
        LoggerMock = new Mock<ILogger<T>>();
        MongoDbContextMock = new FakeMongoDbContext();
        var contextOptions = new DbContextOptionsBuilder<CollectionsDbContext>()
                .UseInMemoryDatabase(databaseName: "InMemoryDatabase")
                .Options;

        CollectionsDbContext = new CollectionsDbContext(contextOptions);
    }

    public Mock<ICqrsMediatr> MediatorMock { get; }
    public Mock<ILogger<T>> LoggerMock;
    public IMongoDbContext MongoDbContextMock { get; }
    public CollectionsDbContext CollectionsDbContext { get; }
}

