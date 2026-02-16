namespace ms_notification.Models;

public class NotificationReceive
{
    public int NotificationId { get; set; }
    public Notification Notification { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; }

    public bool IsRead { get; set; } = false;
    public DateTime? ReadDate { get; set; }
}
