namespace ms_notification.Models
{
    public class Employee
    {
    public int Id { get; set; }
    public string Cin { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }

    public ICollection<Notification> CreatedNotifications { get; set; }
    public ICollection<NotificationReceive> ReceivedNotifications { get; set; }
    }
}
