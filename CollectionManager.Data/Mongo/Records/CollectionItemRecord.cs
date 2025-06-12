namespace xhunter74.CollectionManager.Data.Mongo.Records;

public class CollectionItemRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? CollectionId { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Updated { get; set; }

    public Dictionary<string, object> Fields { get; set; } = new();

    public CollectionItemRecord() { }

    public CollectionItemRecord(DynamicRecord record)
    {
        Id = record.Id;
        if (record.Fields.TryGetValue(CollectionItemConstants.CollectionIdFieldName, out var collectionIdObj))
        {
            if (Guid.TryParse(collectionIdObj.ToString(), out var collectionId))
            {
                CollectionId = collectionId;
            }
        }
        if (record.Fields.TryGetValue(CollectionItemConstants.CreatedFieldName, out var createdObj))
        {
            if (DateTime.TryParse(createdObj.ToString(), out var created))
            {
                Created = created;
            }
        }
        if (record.Fields.TryGetValue(CollectionItemConstants.UpdatedFieldName, out var updatedObj))
        {
            if (DateTime.TryParse(updatedObj.ToString(), out var updated))
            {
                Updated = updated;
            }
        }
        var fields = record.Fields;
        fields.Remove(CollectionItemConstants.CollectionIdFieldName);
        fields.Remove(CollectionItemConstants.CreatedFieldName);
        fields.Remove(CollectionItemConstants.UpdatedFieldName);
        Fields = fields;
    }

    public DynamicRecord ToDynamicRecord()
    {
        var fields = Fields;
        fields.Add(CollectionItemConstants.CollectionIdFieldName, CollectionId.ToString());
        fields.Add(CollectionItemConstants.CreatedFieldName, Created);
        fields.Add(CollectionItemConstants.UpdatedFieldName, Updated);

        var intEntity = new DynamicRecord
        {
            Id = Id,
            Fields = fields,
        };
        return intEntity;
    }
}
