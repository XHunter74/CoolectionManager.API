using System.ComponentModel.DataAnnotations;

namespace xhunter74.CollectionManager.API.Settings;

public class MailJetSettings
{
    public const string ConfigSection = "MailJet";

    [Required(AllowEmptyStrings = false)]
    public string ApiKey { get; set; }
    [Required(AllowEmptyStrings = false)]
    public string SecretKey { get; set; }
}
