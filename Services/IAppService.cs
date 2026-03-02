using ms_notification.DTOs;

namespace ms_notification.Services;

public interface IAppService
{
    public Task<NotificationResponse> SendViaAppAsync<T>(PushNotificationRequest request);
}
