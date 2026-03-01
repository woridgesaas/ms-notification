using ms_notification.Enum;

namespace ms_notification.Models;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Channel Channel { get; set; }
    public NotificationType Type { get; set; }
    public Priority Priority { get; set; }
    public Status Status { get; set; } = Status.CREATED;
    public TemplateCode TemplateCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedBy { get; set; }
    public User User { get; set; } = null!;

    public ICollection<NotificationReceive> Receivers { get; set; } = new List<NotificationReceive>();
}
