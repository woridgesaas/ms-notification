namespace ms_notification.DTOs;

public class SuccessResponse<T>
{
    public T Data { get; set; } = default!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string Message { get; set; } = null!;

}
