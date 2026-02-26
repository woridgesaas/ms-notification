namespace ms_notification.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string Cin { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string FirstName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public ICollection<Notification> CreatedNotifications { get; set; }
            = new List<Notification>();

        public ICollection<NotificationReceive> ReceivedNotifications { get; set; }
            = new List<NotificationReceive>();
    }
}