using System.ComponentModel.DataAnnotations;

namespace xhunter74.CollectionManager.API.Settings;

public class MongoDbSettings
{
    public const string ConfigSection = "MongoDb";
    [Required]
    public string ConnectionString { get; set; }
    [Required]
    public string DatabaseName { get; set; }
}
