using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using ms_notification.Options;

namespace ms_notification.Services;

public class EmailSenderService(IOptions<SmtpSettings> options) : IEmailSenderService
{
    private readonly SmtpSettings _smtpSettings = options.Value;

    // EmailSenderService.cs
public async Task SendEmailAsync(string to, string subject, string body,
    byte[]? attachmentBytes = null, string? attachmentFileName = null)
{
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
    message.To.Add(MailboxAddress.Parse(to));
    message.Subject = subject;

    var builder = new BodyBuilder { HtmlBody = body };

    // ← Pièce jointe PDF si fournie
    if (attachmentBytes is not null && attachmentFileName is not null)
    {
        builder.Attachments.Add(attachmentFileName, attachmentBytes,
    new MimeKit.ContentType("application", "pdf"));
    }

    message.Body = builder.ToMessageBody();

    using var client = new SmtpClient();
    await client.ConnectAsync(_smtpSettings.Server, _smtpSettings.Port, _smtpSettings.UseSsl);

    if (!string.IsNullOrWhiteSpace(_smtpSettings.Username))
        await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);

    await client.SendAsync(message);
    await client.DisconnectAsync(true);
}
    
}