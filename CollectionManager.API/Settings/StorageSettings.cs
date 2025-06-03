using System.ComponentModel.DataAnnotations;

namespace xhunter74.CollectionManager.API.Settings;

public class StorageSettings
{
    public const string ConfigSection = "Storage";

    [Required(AllowEmptyStrings = false)]
    public string StorageFolder { get; set; }
}
