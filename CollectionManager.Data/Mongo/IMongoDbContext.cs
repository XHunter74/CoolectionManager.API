namespace xhunter74.CollectionManager.Data.Mongo;

public interface IMongoDbContext : IDisposable
{
    Task CommitAsync();
}
