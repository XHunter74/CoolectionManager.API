using xhunter74.CollectionManager.API.Models;
using xhunter74.CollectionManager.Data.Entity;
using xhunter74.CollectionManager.Data.Mongo.Records;

namespace xhunter74.CollectionManager.API.Features.Items;

public static class ItemUtils
{
    public static IEnumerable<ItemValue> GetItemValues(Dictionary<string, object> itemFields, ICollection<CollectionField> collectionFields)
    {
        foreach (var field in collectionFields)
        {
            if (itemFields.TryGetValue(field.Id.ToString(), out var valueObj))
            {
                yield return new ItemValue
                {
                    FieldId = field.Id,
                    FieldName = field.DisplayName,
                    Value = valueObj
                };
            }
        }
    }

    public static Guid? GetGuidField(Dictionary<string, object> fields, string fieldName)
    {
        var strValue = GetStringField(fields, fieldName);
        if (Guid.TryParse(strValue, out var guidValue) && guidValue != Guid.Empty)
        {
            return guidValue;
        }
        return null;
    }

    public static string GetStringField(Dictionary<string, object> fields, string fieldName)
    {
        if (fields.TryGetValue(fieldName, out var nameObj))
        {
            return nameObj.ToString();
        }
        return string.Empty;
    }

    internal static ItemDto ConvertMongoItemToItemDto(ICollection<CollectionField> collectionFields,
        CollectionItemRecord itemInDb)
    {
        var item = new ItemDto
        {
            Id = itemInDb.Id,
            CollectionId = itemInDb.CollectionId.Value,
            DisplayName = ItemUtils.GetStringField(itemInDb.Fields, Constants.DisplayNameFieldName),
            Picture = ItemUtils.GetGuidField(itemInDb.Fields, Constants.PictureFieldName),
            Values = ItemUtils.GetItemValues(itemInDb.Fields, collectionFields),
            Created = itemInDb.Created.Value,
            Updated = itemInDb.Updated.Value
        };
        return item;
    }
}
