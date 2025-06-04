using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace xhunter74.CollectionManager.Data.Mongo.Records;

public class DynamicRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();

    [BsonExtraElements]
    public Dictionary<string, object> Fields { get; set; } = new();
}
