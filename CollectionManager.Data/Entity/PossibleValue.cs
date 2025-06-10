namespace xhunter74.CollectionManager.Data.Entity;

public class PossibleValue : BaseEntity
{
    public Guid Id { get; set; }
    public string Value { get; set; }

    public Guid CollectionFieldId { get; set; }
    public CollectionField CollectionField { get; set; }
}
