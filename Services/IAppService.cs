namespace ms_notification.Services;

public interface IAppService
{
    Task SendViaAppAsync<T>(SendNotificationRequest<T> sendNotificationRequest);
}
