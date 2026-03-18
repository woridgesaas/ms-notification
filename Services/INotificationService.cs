using ms_notification.DTOs;
using ms_notification.DTOs.Events;

namespace ms_notification.Services;

public interface INotificationService
{
    Task SendNotificationAsync<T>(SendNotificationRequest<T> sendNotificationRequest);
    Task<NotificationResponse> SendPushNotificationAsync(PushNotificationRequest request);
    Task<List<NotificationResponse>> GetMyNotificationsAsync(string cin);
    Task<bool> MarkAsReadAsync(int notificationId, string cin);
    Task MarkAllAsReadAsync(string cin);
    Task SendResignationNotificationAsync(ResignationCreatedEvent evt);
}
