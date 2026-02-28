using ms_notification.DTO;
using ms_notification.Models;

namespace ms_notification.Mappers;

public static class NotificationMapper
{
    // CreateNotificationRequest -> Notification (insert)
    public static Notification ToEntity(
        CreateNotificationRequest dto, int employeeId)
    {
        return new Notification
        {
            Title = dto.Title,
            Description = dto.Description,
            Channel = dto.Channel,
            Type = dto.Type,
            Priority = dto.Priority,
            CreatedByEmployeeId = employeeId,
            CreationDate = DateTime.UtcNow
        };
    }

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
            CreationDate = n.CreationDate,
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