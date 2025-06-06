using xhunter74.CollectionManager.Data.Mongo.Records;
using xhunter74.CollectionManager.Data.Mongo.Extensions;

namespace CollectionManager.Data.Test.Extensions;

public class DynamicRecordExtensionsTests
{
    [Fact(DisplayName = "ToFlattenedExpando returns all fields and system fields as properties with correct types")]
    public void ToFlattenedExpando_ReturnsExpectedProperties()
    {
        var now = DateTime.UtcNow;
        var record = new DynamicItemRecord
        {
            Id = Guid.NewGuid(),
            CollectionId = Guid.NewGuid(),
            Created = now,
            Updated = now,
            Fields = new Dictionary<string, object>
            {
                { "IntField", 42 },
                { "DecimalField", 42.5m },
                { "StringField", "hello" },
                { "BoolField", true },
                { "DateField", now },
                { "GuidField", Guid.NewGuid() },
                { "NullField", null }
            }
        };
        dynamic expando = record.ToFlattenedExpando();
        Assert.Equal(record.Id, expando.Id);
        Assert.Equal(record.CollectionId, expando.CollectionId);
        Assert.Equal(record.Created, expando.Created);
        Assert.Equal(record.Updated, expando.Updated);
        Assert.Equal(42, expando.IntField);
        Assert.Equal(42.5m, expando.DecimalField);
        Assert.Equal("hello", expando.StringField);
        Assert.Equal(true, expando.BoolField);
        Assert.Equal(now, expando.DateField);
        Assert.IsType<Guid>(expando.GuidField);
        Assert.Null(expando.NullField);
    }

    [Fact(DisplayName = "GetFieldValue returns correct value for existing field and throws for missing field")]
    public void GetFieldValue_WorksAndThrows()
    {
        var record = new DynamicItemRecord
        {
            Fields = new Dictionary<string, object> { { "TestField", 123 } }
        };
        var expando = record.ToFlattenedExpando();
        Assert.Equal(123, expando.GetFieldValue("TestField"));
        Assert.Throws<KeyNotFoundException>(() => expando.GetFieldValue("MissingField"));
    }
}
