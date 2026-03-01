namespace ms_notification.DTOs;

public class ErrorResponse<T>
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public T Message { get; set; } = default!;
}
