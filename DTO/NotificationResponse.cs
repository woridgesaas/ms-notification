namespace ms_notification.DTO;

public class NotificationResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Channel { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Priority { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreationDate { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
}