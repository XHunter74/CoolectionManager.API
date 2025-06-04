using xhunter74.CollectionManager.Data.Mongo.Repositories;

namespace xhunter74.CollectionManager.Data.Mongo;

public interface IMongoDbContext : IDisposable
{
    CollectionsRepository CollectionItems { get; }
    Task CommitAsync();
}
