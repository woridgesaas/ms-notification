namespace ms_notification.Messaging.Events
{
    public class UserCreatedEvent
    {
        public string Cin { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DepartmentId { get; set; }
        public string Role { get; set; }
    }
}
