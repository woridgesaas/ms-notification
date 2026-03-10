using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ms_notification.DTOs;
using ms_notification.DTOs.Email;
using ms_notification.Services;
namespace ms_notification.Controllers;

[Route("api/[controller]")]
[ApiController]

public class NotificationController(INotificationService notificationService) : ControllerBase
{
    private readonly INotificationService _notificationService = notificationService;

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications()
    {
        var cin = User.FindFirst("CIN")?.Value;

        if (cin == null)
            return Unauthorized("CIN missing in token");

        var result = await _notificationService.GetMyNotificationsAsync(cin);

        return Ok(result);
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var cin = User.FindFirst("CIN")?.Value;
        if (cin == null) return Unauthorized();

        var result = await _notificationService.MarkAsReadAsync(id, cin);
        return result ? NoContent() : NotFound();
    }
    
    [HttpPatch]
    [Authorize]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var cin = User.FindFirst("CIN")?.Value;
        if (cin == null) return Unauthorized();

        await _notificationService.MarkAllAsReadAsync(cin);
        return NoContent();
    }

    [HttpPost("otp")]
    public async Task<IActionResult> SendOtpNotification(SendNotificationRequest<OtpData> sendOtpNotificationRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _notificationService.SendNotificationAsync(sendOtpNotificationRequest);
        return Ok(
            new SuccessResponse<string>
            {
                Message = "Notification sent successfully",
                Data = null
            }
        );
    }


    [HttpPost("create-user")]
    public async Task<IActionResult> SendCreateUserNotification(SendNotificationRequest<CreateUserData> sendCreateUserNotificationRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _notificationService.SendNotificationAsync(sendCreateUserNotificationRequest);
        return Ok(
            new SuccessResponse<string>
            {
                Message = "Notification sent successfully",
                Data = null
            }
        );
    }

    [HttpPost("push")]
    public async Task<IActionResult> SendPush([FromBody]PushNotificationRequest request)
    {
        
        NotificationResponse result =await _notificationService.SendPushNotificationAsync(request);
        Console.WriteLine(result + " In Controller");
        return Ok(result);
    }

}