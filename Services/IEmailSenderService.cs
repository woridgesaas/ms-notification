namespace ms_notification.Services;

public interface IEmailSenderService
{
    Task SendEmailAsync(string to, string subject, string body,
        byte[]? attachmentBytes = null, string? attachmentFileName = null);
}
