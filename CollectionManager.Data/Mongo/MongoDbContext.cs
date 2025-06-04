using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;

namespace xhunter74.CollectionManager.Data.Mongo;

public class MongoDbContext : IMongoDbContext
{
    private const string CollectionItemsCollectionName = "collection_items";
    private readonly IClientSessionHandle? _session;
    private bool _disposed;

    public IRepository<BsonDocument> CollectionItems { get; }

    public MongoDbContext(IMongoClient client, string databaseName)
    {
        if (!string.IsNullOrEmpty(client.Settings.ReplicaSetName))
        {
            _session = client.StartSession();
            _session.StartTransaction();
        }

        var database = client.GetDatabase(databaseName);

        var existingCollections = database.ListCollectionNames().ToList();
        if (!existingCollections.Contains(CollectionItemsCollectionName))
        {
            database.CreateCollection(CollectionItemsCollectionName);
        }


        CollectionItems = new MongoRepository<BsonDocument>(database, _session, CollectionItemsCollectionName);
    }

    public async Task CommitAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MongoDbContext));

        if (_session == null)
            throw new InvalidOperationException("Session is not initialized.");

        await _session.CommitTransactionAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _session?.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~MongoDbContext()
    {
        Dispose(false);
    }
}
