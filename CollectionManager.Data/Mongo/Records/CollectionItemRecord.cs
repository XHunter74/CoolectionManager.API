namespace xhunter74.CollectionManager.Data.Mongo.Records;

public class CollectionItemRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CollectionId { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;

    public Dictionary<string, object> Fields { get; set; } = new();

    public CollectionItemRecord() { }

    public CollectionItemRecord(DynamicRecord record)
    {
        Id = record.Id;
        CollectionId = Guid.Parse(record.Fields.GetValueOrDefault(CollectionItemConstants.CollectionIdFieldName, string.Empty)?.ToString());
        Created = DateTime.Parse(record.Fields.GetValueOrDefault(CollectionItemConstants.CreatedFieldName, DateTime.MinValue).ToString());
        Updated = DateTime.Parse(record.Fields.GetValueOrDefault(CollectionItemConstants.UpdatedFieldName, DateTime.MinValue).ToString());
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
