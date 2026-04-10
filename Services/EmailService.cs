using ms_notification.Enum;
using ms_notification.Exceptions;
using ms_notification.Extensions;
using ms_notification.Models;
using ms_notification.Repositories;

namespace ms_notification.Services;

public class EmailService(INotificationRepository notificationRepository, IUserRepository userRepository, ITemplateRendererService templateRendererService, IEmailSenderService emailSenderService) : IEmailService
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITemplateRendererService _templateRendererService = templateRendererService;
    private readonly IEmailSenderService _emailSenderService = emailSenderService;


    public async Task SendViaEmailAsync<T>(SendNotificationRequest<T> request)
    {
        User? user = null;
        if (!string.IsNullOrEmpty(request.CreatedBy) && request.CreatedBy != "SYSTEM")
        {
            user = await _userRepository.GetUserByCINAsync(request.CreatedBy);
        }

        string body = await _templateRendererService.RenderTemplateAsync(
                             request.TemplateCode, request.Data);
        string subject = TemplateCodeExtension.GetSubject(request.TemplateCode);

        await _emailSenderService.SendEmailAsync(request.To, subject, body);

        // ← Sauvegarde seulement si user existe
        if (user != null)
        {
            Notification notification = new()
            {
                Title = subject,
                Channel = Channel.EMAIL,
                Type = NotificationType.NOTIFICATION,
                Priority = Priority.NORMAL,
                Status = Status.SENT,
                TemplateCode = request.TemplateCode,
                CreatedBy = user.Id
            };
            await _notificationRepository.AddNotificationAsync(notification);
        }
    }
}