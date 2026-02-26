using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using ms_notification.Options;

namespace ms_notification.Services;

public class EmailService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpOptions)
    {
        _smtpSettings = smtpOptions.Value;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, false);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}