using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PrivacyConsent.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly string _fromAddress;
    private readonly string _denmarkNotificationEmail;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string? _smtpUsername;
    private readonly string? _smtpPassword;

    private static readonly Dictionary<string, (string Subject, string Template)> EmailTexts = new()
    {
        ["objection"] = (
            "Data processing objection request",
            "User with CONNECT ID {0} and email {1} has raised a objection request leaving the following note: {2}"),
        ["restriction"] = (
            "Data processing restriction request",
            "User with CONNECT ID {0} and email {1} has raised a data processing restriction request leaving the following note: {2}")
    };

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _logger = logger;
        _fromAddress = config["EmailService:From"] ?? "privacy@consents.privacy.telenordigital.com";
        _denmarkNotificationEmail = config["EmailService:DenmarkNotificationEmail"] ?? "";
        _smtpHost = config["Email:SmtpHost"] ?? "";
        _smtpPort = config.GetValue("Email:SmtpPort", 587);
        _smtpUsername = config["Email:SmtpUsername"];
        _smtpPassword = config["Email:SmtpPassword"];
    }

    public async Task<string?> SendEmailAsync(string from, string to, string subject, string body)
    {
        _logger.LogInformation("Sending email from {From} to {To}: {Subject}", from, to, subject);

        if (string.IsNullOrEmpty(_smtpHost))
        {
            _logger.LogWarning("SMTP host not configured. Email not sent (from: {From}, to: {To}, subject: {Subject})", from, to, subject);
            return Guid.NewGuid().ToString();
        }

        try
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort);

            if (!string.IsNullOrEmpty(_smtpUsername) && !string.IsNullOrEmpty(_smtpPassword))
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

            client.EnableSsl = _smtpPort != 25;

            var message = new MailMessage(from, to, subject, body);
            await client.SendMailAsync(message);

            var messageId = Guid.NewGuid().ToString();
            _logger.LogInformation("Email sent successfully. MessageId: {MessageId}", messageId);
            return messageId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email from {From} to {To}: {Subject}", from, to, subject);
            return null;
        }
    }

    public async Task SendDsrNotificationEmailAsync(string userId, string email, string right, string? note)
    {
        if (EmailTexts.TryGetValue(right, out var template))
        {
            var body = string.Format(template.Template, userId, email, note ?? "");
            await SendEmailAsync(_fromAddress, _denmarkNotificationEmail, template.Subject, body);
            _logger.LogInformation("DSR notification sent for user {UserId}: {Right}", userId, right);
        }
        else
        {
            _logger.LogError("Unknown DSR request type for email: {Right} (userId: {UserId})", right, userId);
        }
    }
}
