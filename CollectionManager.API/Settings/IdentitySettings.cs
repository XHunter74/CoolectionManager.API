using System.ComponentModel.DataAnnotations;

namespace xhunter74.CollectionManager.API.Settings;

public class IdentitySettings
{
    public const string ConfigSection = "Identity";

    [Required(AllowEmptyStrings = false)]
    public string DefaultClientId { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string DefaultClientSecret { get; set; }
    //TODO Return [Range(60, 3600, ErrorMessage = "Access token lifetime must be between 60 and 3600 seconds.")]
    [Required]
    [Range(60, 10800, ErrorMessage = "Access token lifetime must be between 60 and 3600 seconds.")]
    public int AccessTokenLifetime { get; set; }
    [Required]
    [Range(3600, 10080, ErrorMessage = "Refresh token lifetime must be between 3600 and 10800 seconds.")]
    public int RefreshTokenLifetime { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string SuperAdminEmail { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "Super admin password is required.")]
    public string SuperAdminPassword { get; set; }
}
