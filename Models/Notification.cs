using ms_notification.Enum;

namespace ms_notification.Models;

public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Channel Channel { get; set; }
    public TypeNotif Type { get; set; }
    public PriorityNotif Priority { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public StatusNotif Status { get; set; }

    public int CreatedByEmployeeId { get; set; }
    public Employee CreatedByEmployee { get; set; }

    public ICollection<NotificationReceive> Receivers { get; set; }//Relation Many-to-Many avec attributs
}
