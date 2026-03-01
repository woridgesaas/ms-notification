namespace ms_notification.Services;

public interface INotificationService
{
    Task SendNotificationAsync<T>(SendNotificationRequest<T> sendNotificationRequest);
}
