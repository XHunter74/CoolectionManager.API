using MongoDB.Bson;
using MongoDB.Driver;

namespace xhunter74.CollectionManager.Data.Mongo;

public class MongoRepository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly IClientSessionHandle? _session;

    public MongoRepository(IMongoDatabase database, IClientSessionHandle? session, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
        _session = session;
    }

    public async Task<IEnumerable<T>> GetByFilter(FilterDefinition<T> filter)
    {
        var documents = _session == null ? await _collection.Find(filter).ToListAsync()
            : await _collection.Find(_session, filter).ToListAsync();
        return documents;
    }

    public async Task<T> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        var document = _session == null ? await _collection.Find(filter).FirstOrDefaultAsync()
            : await _collection.Find(_session, filter).FirstOrDefaultAsync();
        return document;
    }

    public async Task<T> AddAsync(T entity)
    {
        if (_session == null)
        {
            await _collection.InsertOneAsync(entity);
        }
        else
        {
            await _collection.InsertOneAsync(_session, entity);
        }

        return entity;
    }

    public async Task<bool> RemoveAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        DeleteResult result;

        if (_session == null)
            result = await _collection.DeleteOneAsync(filter);
        else
            result = await _collection.DeleteOneAsync(_session, filter);

        return result.DeletedCount > 0;
    }

    public async Task<IEnumerable<T>> GetAllCollectionItemsAsync(string collectionId)
    {
        var filter = Builders<T>.Filter.Eq("collectionId", collectionId);
        var allDocuments = _session == null ? await _collection.Find(filter).ToListAsync()
            : await _collection.Find(_session, filter).ToListAsync();
        return allDocuments;
    }

    public async Task<bool> UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        ReplaceOneResult result;

        if (_session != null)
            result = await _collection.ReplaceOneAsync(_session, filter, entity);
        else
            result = await _collection.ReplaceOneAsync(filter, entity);
        return result.MatchedCount > 0;
    }
}
