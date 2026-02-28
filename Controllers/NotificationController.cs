using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ms_notification.DTO;
using ms_notification.Services;

namespace ms_notification.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotifService _service;

    public NotificationController(INotifService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateNotificationRequest request)
    {
        var cin = User.FindFirst("CIN")?.Value;

        if (cin == null)
            return Unauthorized("CIN missing in token");

        await _service.CreateNotificationAsync(request, cin);

        return Ok(new { message = "Notification created successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var cin = User.FindFirst("CIN")?.Value;

        if (cin == null)
            return Unauthorized("CIN missing in token");

        var result = await _service.GetMyNotificationsAsync(cin);

        return Ok(result);
    }

    // PATCH /api/notifications/{id}/read
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var cin = User.FindFirst("CIN")?.Value;
        if (cin == null) return Unauthorized();

        var result = await _service.MarkAsReadAsync(id, cin);
        return result ? NoContent() : NotFound();
    }

    // PATCH /api/notifications/read-all
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var cin = User.FindFirst("CIN")?.Value;
        if (cin == null) return Unauthorized();

        await _service.MarkAllAsReadAsync(cin);
        return NoContent();
    }
}