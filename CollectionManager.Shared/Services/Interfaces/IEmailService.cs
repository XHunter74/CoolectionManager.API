namespace xhunter74.CollectionManager.Shared.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}
