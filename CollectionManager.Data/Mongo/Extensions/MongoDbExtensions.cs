using MongoDB.Driver;
using xhunter74.CollectionManager.Data.Mongo.Records;

namespace xhunter74.CollectionManager.Data.Mongo.Extensions;

public static class MongoDbExtensions
{
    public static IMongoDatabase CreatedMongoDb(this IMongoClient mongoClient, string databaseName)
    {
        var database = mongoClient.GetDatabase(databaseName);

        var existingCollections = database.ListCollectionNames().ToList();

        if (!existingCollections.Contains(MongoConstants.CollectionItemsCollectionName))
        {
            database.CreateCollection(MongoConstants.CollectionItemsCollectionName);
        }

        return database;
    }

    public static IMongoDatabase CreateDbIndexes(this IMongoDatabase database)
    {
        var intCollections = database.GetCollection<DynamicItemRecord>(MongoConstants.CollectionItemsCollectionName);

        var existingIndexes = intCollections.Indexes.List().ToList();

        bool hasCollectionIdIndex = existingIndexes.Any(idx =>
            idx["name"].AsString == MongoConstants.CollectionIndexName
        );

        if (!hasCollectionIdIndex)
        {
            var indexDefinition = Builders<DynamicItemRecord>.IndexKeys.Ascending(p => p.CollectionId);

            var indexOptions = new CreateIndexOptions
            {
                Name = MongoConstants.CollectionIndexName,
                Background = true
            };

            var indexModel = new CreateIndexModel<DynamicItemRecord>(indexDefinition, indexOptions);

            intCollections.Indexes.CreateOne(indexModel);
        }
        return database;
    }
}
