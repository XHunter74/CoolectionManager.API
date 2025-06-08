using System.ComponentModel.DataAnnotations;
using xhunter74.CollectionManager.Shared.Services;

namespace xhunter74.CollectionManager.API.Settings;

public class AppSettings
{
    public const string ConfigSection = "AppSettings";

    [Required]
    public StorageServices StorageService { get; set; }
    [Required]
    [EmailAddress]
    public string FromEmail { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string FromName { get; set; }
}
