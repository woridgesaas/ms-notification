using ms_notification.DTOs;
using ms_notification.Models;

namespace ms_notification.Mappers;

public static class NotificationMapper
{
    // NotificationReceive -> NotificationResponse 
    public static NotificationResponse ToResponse(NotificationReceive receive)
    {
        var n = receive.Notification;

        return new NotificationResponse
        {
            Id = n.Id,
            Title = n.Title,
            Description = n.Description,
            Channel = n.Channel.ToString(),
            Type = n.Type.ToString(),
            Priority = n.Priority.ToString(),
            Status = n.Status.ToString(),
            CreationDate = n.CreatedAt,
            IsRead = receive.IsRead,
            ReadDate = receive.ReadDate
        };
    }

    // Liste NotificationReceive -> Liste NotificationResponse
    public static List<NotificationResponse> ToResponseList(
        IEnumerable<NotificationReceive> receives)
    {
        return receives.Select(ToResponse).ToList();
    }
}