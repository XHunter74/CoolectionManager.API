using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using xhunter74.CollectionManager.Shared.Services.Interfaces;

namespace xhunter74.CollectionManager.Shared.Services;

public class MailJetService : IEmailService
{
    private readonly ILogger<MailJetService> _logger;
    private readonly MailjetClient _mailjetClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public MailJetService(
        ILogger<MailJetService> logger,
        string apiKey,
        string secretKey,
        string fromEmail,
        string fromName
        )
    {
        _logger = logger;
        _mailjetClient = new MailjetClient(apiKey, secretKey);
        _fromEmail = fromEmail;
        _fromName = fromName;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var request = new MailjetRequest
        {
            Resource = Send.Resource,
        }
        .Property(Send.Messages, new JArray {
            new JObject {
                {"From", new JObject {
                    {"Email", _fromEmail},
                    {"Name", _fromName}
                  }},
                {"To", new JArray {
                    new JObject {
                        {"Email", to},
                        }
                    }},
                    {"Subject", subject},
                    {"HTMLPart", body}
                }
            });
        var response = await _mailjetClient.PostAsync(request);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount());
            _logger.LogInformation("Email successfully send to '{0}'", to);
        }
        else
        {
            _logger.LogError("Failed to send email to '{0}'. Status: {1}, Error: {2}",
                to, response.StatusCode, response.GetErrorInfo());
            _logger.LogError("Error message: {0}", response.GetErrorMessage());
        }
    }
}
