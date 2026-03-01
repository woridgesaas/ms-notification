using Microsoft.AspNetCore.Mvc;
using ms_notification.DTOs;
using ms_notification.DTOs.Email;
using ms_notification.Services;

namespace ms_notification.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    private readonly INotificationService _notificationService = notificationService;

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


}