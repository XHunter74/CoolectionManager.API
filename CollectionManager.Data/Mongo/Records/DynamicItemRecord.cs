namespace xhunter74.CollectionManager.Data.Mongo.Records;

public class DynamicItemRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CollectionId { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;

    public Dictionary<string, object> Fields { get; set; } = new();

    public DynamicItemRecord() { }

    public DynamicItemRecord(DynamicRecord record)
    {
        Id = record.Id;
        CollectionId = Guid.Parse(record.Fields.GetValueOrDefault(ItemConstants.CollectionIdFieldName, string.Empty)?.ToString());
        Created = DateTime.Parse(record.Fields.GetValueOrDefault(ItemConstants.CreatedFieldName, DateTime.MinValue).ToString());
        Updated = DateTime.Parse(record.Fields.GetValueOrDefault(ItemConstants.UpdatedFieldName, DateTime.MinValue).ToString());
        var fields = record.Fields;
        fields.Remove(ItemConstants.CollectionIdFieldName);
        fields.Remove(ItemConstants.CreatedFieldName);
        fields.Remove(ItemConstants.UpdatedFieldName);
        Fields = fields;
    }

    public DynamicRecord ToDynamicRecord()
    {
        var fields = Fields;
        fields.Add(ItemConstants.CollectionIdFieldName, CollectionId.ToString());
        fields.Add(ItemConstants.CreatedFieldName, Created);
        fields.Add(ItemConstants.UpdatedFieldName, Updated);

        var intEntity = new DynamicRecord
        {
            Id = Id,
            Fields = fields,
        };
        return intEntity;
    }
}
