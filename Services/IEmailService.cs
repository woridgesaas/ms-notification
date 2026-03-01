namespace ms_notification.Services;

public interface IEmailService
{
    Task SendViaEmailAsync<T>(SendNotificationRequest<T> sendNotificationRequest);
}
