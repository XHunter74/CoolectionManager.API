namespace xhunter74.CollectionManager.API.Models;

public class CreateItemDto
{
    public string DisplayName { get; set; }
    public Guid? Picture { get; set; }
    public IEnumerable<CreateItemValue> Values { get; set; } = new List<CreateItemValue>();
}

public class CreateItemValue
{
    public Guid FieldId { get; set; }
    public object? Value { get; set; }
}