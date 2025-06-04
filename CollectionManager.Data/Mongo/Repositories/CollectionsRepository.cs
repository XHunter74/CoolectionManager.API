using MongoDB.Bson;
using MongoDB.Driver;
using xhunter74.CollectionManager.Data.Mongo.Records;

namespace xhunter74.CollectionManager.Data.Mongo.Repositories;

public class CollectionsRepository : IRepository<DynamicItemRecord>
{
    protected readonly IMongoCollection<DynamicRecord> _collection;
    protected readonly IClientSessionHandle? _session;

    public CollectionsRepository(IMongoDatabase database, IClientSessionHandle? session, string collectionName)
    {
        _collection = database.GetCollection<DynamicRecord>(collectionName);
        _session = session;
    }

    public async Task<DynamicItemRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<DynamicRecord>.Filter.Eq("_id", id.ToString());
        var intDocument = _session == null ? await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken)
            : await _collection.Find(_session, filter).FirstOrDefaultAsync(cancellationToken);
        var document = intDocument == null ? null : new DynamicItemRecord(intDocument);
        return document;
    }

    public async Task<DynamicItemRecord> AddAsync(DynamicItemRecord entity, CancellationToken cancellationToken)
    {
        var intEntity = entity.ToDynamicRecord();

        if (_session == null)
        {
            await _collection.InsertOneAsync(intEntity, cancellationToken: cancellationToken);
        }
        else
        {
            await _collection.InsertOneAsync(_session, intEntity, cancellationToken: cancellationToken);
        }

        return entity;
    }

    public async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<DynamicRecord>.Filter.Eq("_id", id.ToString());
        DeleteResult result;

        if (_session == null)
            result = await _collection.DeleteOneAsync(filter, cancellationToken);
        else
            result = await _collection.DeleteOneAsync(_session, filter, cancellationToken: cancellationToken);

        return result.DeletedCount > 0;
    }

    public async Task<IEnumerable<DynamicItemRecord>> GetAllCollectionItemsAsync(Guid collectionId, CancellationToken cancellationToken)
    {
        var filter = Builders<DynamicRecord>.Filter.Eq(ItemConstants.CollectionIdFieldName, collectionId.ToString());
        var allIntDocuments = _session == null ? await _collection.Find(filter).ToListAsync(cancellationToken)
            : await _collection.Find(_session, filter).ToListAsync(cancellationToken);

        var allDocuments = allIntDocuments.Select(d => new DynamicItemRecord(d))
            .ToList();

        return allDocuments;
    }

    public async Task<bool> UpdateAsync(Guid id, DynamicItemRecord entity, CancellationToken cancellationToken)
    {
        var filter = Builders<DynamicRecord>.Filter.Eq("_id", id.ToString());

        ReplaceOneResult result;

        var intEntity = entity.ToDynamicRecord();

        if (_session != null)
            result = await _collection.ReplaceOneAsync(_session, filter, intEntity, cancellationToken: cancellationToken);
        else
            result = await _collection.ReplaceOneAsync(filter, intEntity, cancellationToken: cancellationToken);

        return result.MatchedCount > 0;
    }
}
