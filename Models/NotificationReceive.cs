namespace ms_notification.Models;

public class NotificationReceive
{
    public int NotificationId { get; set; }
    public Notification Notification { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public bool IsRead { get; set; } = false;
    public DateTime? ReadDate { get; set; }
}
