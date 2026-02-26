using ms_notification.Enum;

namespace ms_notification.Models;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Channel Channel { get; set; }
    public TypeNotif Type { get; set; }
    public PriorityNotif Priority { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public StatusNotif Status { get; set; } = StatusNotif.Created;

    //  Relation Template (important pour traçabilité)
    public int? TemplateId { get; set; }
    public NotificationTemplate? Template { get; set; }

    public int CreatedByEmployeeId { get; set; }
    public Employee CreatedByEmployee { get; set; }

    public ICollection<NotificationReceive> Receivers { get; set; } = new List<NotificationReceive>();
}
