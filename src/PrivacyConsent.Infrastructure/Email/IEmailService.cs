namespace PrivacyConsent.Infrastructure.Email;

public interface IEmailService
{
    Task<string?> SendEmailAsync(string from, string to, string subject, string body);
    Task SendDsrNotificationEmailAsync(string userId, string email, string right, string? note);
}
