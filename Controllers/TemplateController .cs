using Microsoft.AspNetCore.Mvc;
using ms_notification.Services;
using ms_notification.DTO;

namespace ms_notification.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
    {
        await _notificationService.SendAsync(
            request.TemplateName,
            request.Channel.ToString(),
            request.ToEmail,
            request.Data);

        return Ok("Notification sent successfully");
    }
}