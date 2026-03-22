using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using ms_notification.Options;

namespace ms_notification.Services;

public class EmailSenderService(IOptions<SmtpSettings> options) : IEmailSenderService
{
    private readonly SmtpSettings _smtpSettings = options.Value;

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, _smtpSettings.UseSsl);

        if (!string.IsNullOrWhiteSpace(_smtpSettings.Username))
        {
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}