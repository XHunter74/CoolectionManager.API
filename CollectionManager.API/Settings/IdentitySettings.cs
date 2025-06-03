using System.ComponentModel.DataAnnotations;

namespace xhunter74.CollectionManager.API.Settings;

public class IdentitySettings
{
    public const string ConfigSection = "Identity";

    [Required(AllowEmptyStrings = false)]
    public string DefaultClientId { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string DefaultClientSecret { get; set; }
    [Required]
    [Range(60, 3600, ErrorMessage = "Access token lifetime must be between 60 and 3600 seconds.")]
    public int AccessTokenLifetime { get; set; }
    [Required]
    [Range(3600, 10080, ErrorMessage = "Refresh token lifetime must be between 3600 and 10800 seconds.")]
    public int RefreshTokenLifetime { get; set; }
}
