namespace ms_notification.Services;

public class AppService : IAppService
{
    public async Task SendViaAppAsync<T>(SendNotificationRequest<T> sendNotificationRequest)
    {
        throw new NotImplementedException();
    }
}
