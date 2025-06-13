namespace xhunter74.CollectionManager.API.Models;

public class ItemDto
{
    public Guid Id { get; set; }
    public Guid? CollectionId { get; set; }
    public string? DisplayName { get; set; }
    public Guid? Picture { get; set; }
    public IEnumerable<ItemValue> Values { get; set; } = new List<ItemValue>();

    public DateTime? Created { get; set; }
    public DateTime? Updated { get; set; }
}

public class ItemValue
{
    public Guid FieldId { get; set; }
    public string FieldName { get; set; }
    public object Value { get; set; }
}
