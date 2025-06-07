using System.Dynamic;
using xhunter74.CollectionManager.Data.Mongo.Records;

namespace xhunter74.CollectionManager.Data.Mongo.Extensions;

public static class DynamicItemRecordExtensions
{
    public static ExpandoObject ToFlattenedExpando(this CollectionItemRecord record)
    {
        var result = new ExpandoObject() as IDictionary<string, object>;

        result[nameof(record.Id)] = record.Id;
        result[nameof(record.CollectionId)] = record.CollectionId;
        result[nameof(record.Created)] = record.Created;
        result[nameof(record.Updated)] = record.Updated;

        var decimalFormat = new System.Globalization.NumberFormatInfo
        {
            NumberDecimalSeparator = "."
        };

        foreach (var field in record.Fields)
        {
            var key = field.Key;
            var rawValue = field.Value;

            if (rawValue == null)
            {
                result[key] = null;
                continue;
            }

            var type = rawValue.GetType();
            if (type == typeof(int)
             || type == typeof(long)
             || type == typeof(decimal)
             || type == typeof(double)
             || type == typeof(bool)
             || type == typeof(DateTime)
             || type == typeof(Guid))
            {
                result[key] = rawValue;
                continue;
            }

            var str = rawValue.ToString();

            if (decimal.TryParse(str, System.Globalization.NumberStyles.Any, decimalFormat, out var dec))
            {
                if (dec % 1 == 0)
                    result[key] = (int)dec;
                else
                    result[key] = dec;
                continue;
            }

            if (int.TryParse(str, out var i))
            {
                result[key] = i;
                continue;
            }

            if (bool.TryParse(str, out var b))
            {
                result[key] = b;
                continue;
            }

            if (DateTime.TryParse(str, out var dt))
            {
                result[key] = dt;
                continue;
            }

            if (Guid.TryParse(str, out var guid))
            {
                result[key] = guid;
                continue;
            }
            result[key] = str;
        }

        return (ExpandoObject)result;
    }

    public static object GetFieldValue(this ExpandoObject expando, string fieldName)
    {
        if (expando is IDictionary<string, object> dict && dict.TryGetValue(fieldName, out var value))
        {
            return value;
        }
        throw new KeyNotFoundException($"Field '{fieldName}' not found in the ExpandoObject.");
    }
}