using ms_notification.Enum;
using ms_notification.Exceptions;
namespace ms_notification.Services;

public class NotificationService(IEmailService emailService, IAppService appService) : INotificationService
{
    private readonly IEmailService _emailService = emailService;
    private readonly IAppService _appService = appService;

    public async Task SendNotificationAsync<T>(SendNotificationRequest<T> sendNotificationRequest)
    {
        switch (sendNotificationRequest.Channel)
        {
            case Channel.EMAIL:
                await _emailService.SendViaEmailAsync(sendNotificationRequest);
                break;

            case Channel.APP:
                await _appService.SendViaAppAsync(sendNotificationRequest);
                break;

            default:
                throw new NotificationChannelNotSupportedException("The following channel : " + sendNotificationRequest.Channel + " is not supported");
        }
    }
}