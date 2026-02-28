using ms_notification.Enum;

public class CreateNotificationRequest
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Channel Channel { get; set; }
    public TypeNotif Type { get; set; }
    public PriorityNotif Priority { get; set; }
}