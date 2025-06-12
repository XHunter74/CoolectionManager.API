using MongoDB.Driver;
using xhunter74.CollectionManager.Data.Mongo.Records;

namespace xhunter74.CollectionManager.Data.Mongo.Repositories;

public class CollectionsRepository : IRepository<CollectionItemRecord>
{
    protected readonly IMongoCollection<DynamicRecord> _collection;
    protected readonly IClientSessionHandle? _session;

    public CollectionsRepository(IMongoDatabase database, IClientSessionHandle? session, string collectionName)
    {
        _collection = database.GetCollection<DynamicRecord>(collectionName);
        _session = session;
    }

    public virtual async Task<CollectionItemRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<DynamicRecord>.Filter.Eq("_id", id.ToString());
        var intDocument = _session == null ? await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken)
            : await _collection.Find(_session, filter).FirstOrDefaultAsync(cancellationToken);
        var document = intDocument == null ? null : new CollectionItemRecord(intDocument);
        return document;
    }

    public virtual async Task<CollectionItemRecord> AddAsync(CollectionItemRecord entity, CancellationToken cancellationToken)
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

    public virtual async Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<DynamicRecord>.Filter.Eq("_id", id.ToString());
        DeleteResult result;

        if (_session == null)
            result = await _collection.DeleteOneAsync(filter, cancellationToken);
        else
            result = await _collection.DeleteOneAsync(_session, filter, cancellationToken: cancellationToken);

        return result.DeletedCount > 0;
    }

    public virtual async Task<IEnumerable<CollectionItemRecord>> GetAllCollectionItemsAsync(Guid collectionId,
        IEnumerable<string>? fieldsToInclude, CancellationToken cancellationToken)
    {
        var filter = Builders<DynamicRecord>.Filter.Eq(CollectionItemConstants.CollectionIdFieldName, collectionId.ToString());

        var find = _session == null
            ? _collection.Find(filter)
            : _collection.Find(_session, filter);

        if (fieldsToInclude != null && fieldsToInclude.Any())
        {
            var projDef = Builders<DynamicRecord>.Projection.Include(fieldsToInclude.First());
            foreach (var field in fieldsToInclude.Skip(1))
                projDef = projDef.Include(field);

            find = find.Project<DynamicRecord>(projDef);
        }

        var allIntDocuments = _session == null
            ? await find.ToListAsync(cancellationToken)
            : await find.ToListAsync(cancellationToken);

        var allDocuments = allIntDocuments.Select(d => new CollectionItemRecord(d))
            .ToList();

        return allDocuments;
    }

    public virtual async Task<bool> UpdateAsync(Guid id, CollectionItemRecord entity, CancellationToken cancellationToken)
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
