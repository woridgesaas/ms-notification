using Microsoft.AspNetCore.SignalR;
using ms_notification.DTOs;
using ms_notification.Enum;
using ms_notification.Exceptions;
using ms_notification.Hubs;
using ms_notification.Mappers;
using ms_notification.Models;
using ms_notification.Repositories;
using RazorLight;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ms_notification.Services;

public class AppService : IAppService
{
    private readonly IHubContext<NotificationHub> _contextHub;
    private readonly ITemplateRendererService _templateRenderer;
    private readonly ITemplateProvider _templateProvider;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<AppService> _logger;
    public AppService(IHubContext<NotificationHub> contextHub ,ITemplateRendererService templateRendererService , ITemplateProvider templateProvider , IUserRepository userRepository , INotificationRepository notifRepo, ILogger<AppService> logger)
    {
        this._contextHub = contextHub;
        this._templateProvider = templateProvider;
        this._templateRenderer = templateRendererService;
        this._userRepository = userRepository;
        this._notificationRepository = notifRepo;
        this._logger = logger;
    }
    public async Task<NotificationResponse> SendViaAppAsync<T>(PushNotificationRequest request)
    {
        var templateContent = await _templateProvider.GetTemplateAsync(request.TemplateName, request.Channel);
        var body = _templateRenderer.RenderTemplate(templateContent, request.Data);

        var user = await _userRepository.GetUserByCINAsync(request.TargetCin)
                   ?? throw new UserNotFoundException("User not found");
        this._logger.LogWarning("User with CIN {CIN} while sending push notification", request.TargetCin);
        var notification = new Notification
        {
            Title = request.TemplateName,
            Description = body,
            Channel = request.Channel,
            Type = request.NotificationType,
            Priority = request.Priority,
            TemplateCode = TemplateCode.CUSTOM,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id, // ? utilise l'id du user cible
            Status = Status.CREATED
        };

        var receiveEntry = new NotificationReceive
        {
            UserId = user.Id,
            IsRead = false,
            Notification = notification //link in memory
        };


        await _notificationRepository.AddNotificationAsync(notification);
        notification.Receivers.Add(receiveEntry);

        await _notificationRepository.SaveChangesAsync();
        var response = NotificationMapper.ToResponse(receiveEntry);

        // 6. Push SignalR
        await _contextHub.Clients.User(request.TargetCin).SendAsync("ReceiveNotification", response);
        this._logger.LogInformation("Push Notification sent to user with CIN {CIN} , notification Id {notifId}", request.TargetCin,response.Id );
        return response;
    }

}
