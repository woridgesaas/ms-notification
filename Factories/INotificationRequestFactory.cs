using ms_notification.DTOs.Email;
using ms_notification.Enum;
using ms_notification.Messaging.Events;

namespace ms_notification.Factories
{
    public interface INotificationRequestFactory
    {
       
        SendNotificationRequest<TData> CreateRequest<TData>(AbstractEvent ev , TemplateCode templateCode)
          where TData:class;

    }
}
