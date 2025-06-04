using MongoDB.Bson;

namespace xhunter74.CollectionManager.Data.Mongo;

public interface IMongoDbContext : IDisposable
{
    IRepository<BsonDocument> CollectionItems { get; }
    Task CommitAsync();
}
