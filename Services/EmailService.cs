using ms_notification.Enum;
using ms_notification.Exceptions;
using ms_notification.Extensions;
using ms_notification.Models;
using ms_notification.Repositories;
using ms_notification.Messaging.Events;  // ← ajouter

namespace ms_notification.Services;

public class EmailService(INotificationRepository notificationRepository, IUserRepository userRepository, ITemplateRendererService templateRendererService, IEmailSenderService emailSenderService , IPdfGeneratorService pdfGeneratorService) : IEmailService
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly ITemplateRendererService _templateRendererService = templateRendererService;
    private readonly IEmailSenderService _emailSenderService = emailSenderService;
    private readonly IPdfGeneratorService _pdfGeneratorService = pdfGeneratorService;


    public async Task SendViaEmailAsync<T>(SendNotificationRequest<T> request)
    {
        User? user = null;
        if (!string.IsNullOrEmpty(request.CreatedBy) && request.CreatedBy != "SYSTEM")
            user = await _userRepository.GetUserByCINAsync(request.CreatedBy);

        string body = await _templateRendererService.RenderTemplateAsync(
            request.TemplateCode, request.Data);
        string subject = TemplateCodeExtension.GetSubject(request.TemplateCode);

        // ← Générer PDF seulement pour CONTRACT_GENERATED
        byte[]? pdfBytes = null;
        string? pdfFileName = null;

        if (request.TemplateCode == TemplateCode.CONTRACT_GENERATED
    && request.Data is ContractGeneratedEvent contractEvent)
{
    pdfBytes    = await pdfGeneratorService.GenerateFromHtmlAsync(contractEvent.ContractHtml);
    pdfFileName = $"Contrat_{contractEvent.CandidateName.Replace(" ", "_")}.pdf";
}

        await emailSenderService.SendEmailAsync(request.To, subject, body, pdfBytes, pdfFileName);

        if (user != null)
        {
            var notification = new Notification
            {
                Title    = subject,
                Channel  = Channel.EMAIL,
                Type     = NotificationType.NOTIFICATION,
                Priority = Priority.NORMAL,
                Status   = Status.SENT,
                TemplateCode = request.TemplateCode,
                CreatedBy    = user.Id
            };
            await _notificationRepository.AddNotificationAsync(notification);
        }
    }
}
    
