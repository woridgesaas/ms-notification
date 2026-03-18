using ms_notification.DTOs;
using ms_notification.DTOs.Email;
using ms_notification.DTOs.Events;
using ms_notification.Enum;
using ms_notification.Exceptions;
using ms_notification.Mappers;
using ms_notification.Models;
using ms_notification.Repositories;
namespace ms_notification.Services;

public class NotificationService(IEmailService emailService, IAppService appService, IUserRepository userRepository, INotificationRepository notificationRepository) : INotificationService
{
    private readonly IEmailService _emailService = emailService;
    private readonly IAppService _appService = appService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly INotificationRepository _notificationRepository = notificationRepository;


    public async Task SendNotificationAsync<T>(SendNotificationRequest<T> sendNotificationRequest)
    {
        switch (sendNotificationRequest.Channel)
        {
            case Channel.EMAIL:
                await _emailService.SendViaEmailAsync(sendNotificationRequest);
                break;

            /**case Channel.APP:
                await _appService.SendViaAppAsync<PushNotificationRequest>(request);
                break;*/

            default:
                throw new NotificationChannelNotSupportedException("The following channel : " + sendNotificationRequest.Channel + " is not supported");
        }
    }
    
    public async Task<NotificationResponse> SendPushNotificationAsync(PushNotificationRequest request)
    {
       
             return await _appService.SendViaAppAsync<PushNotificationRequest>(request);
    }

    public async Task<List<NotificationResponse>> GetMyNotificationsAsync(string cin)
    {
        User user = await _userRepository.GetUserByCINAsync(cin) ?? throw new UserNotFoundException("User with the following CIN : " + cin + " not found");

        var receives = await _notificationRepository.GetByEmployeeIdAsync(user.Id);

        return NotificationMapper.ToResponseList(receives);
    }
    public async Task<bool> MarkAsReadAsync(int notificationId, string cin)
    {
        User user = await _userRepository.GetUserByCINAsync(cin) ?? throw new UserNotFoundException("User with the following CIN : " + cin + " not found");

        var receive = await _notificationRepository.GetReceiveAsync(notificationId, user.Id) ?? throw new NotificationNotFoundException("Notification with the following ID : " + notificationId + " not found for user with the following CIN : " + cin);

        receive.IsRead = true;
        receive.ReadDate = DateTime.UtcNow;

        await _notificationRepository.UpdateReceiveAsync(receive);
        return true;
    }

    public async Task MarkAllAsReadAsync(string cin)
    {
        User user = await _userRepository.GetUserByCINAsync(cin) ?? throw new UserNotFoundException("User with the following CIN : " + cin + " not found");

        List<NotificationReceive> receives = await _notificationRepository.GetUnreadByEmployeeIdAsync(user.Id);

        foreach (var notification in receives)
        {
            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
        }

        await _notificationRepository.SaveChangesAsync();
    }

    public async Task SendResignationNotificationAsync(ResignationCreatedEvent evt)
    {
        var request = new SendNotificationRequest<ResignationData>
        {
            To = evt.EmployeeEmail,
            CreatedBy = evt.EmployeeCin,
            Channel = Channel.EMAIL,
            TemplateCode = TemplateCode.RESIGNATION_CONFIRMATION,
            Data = new ResignationData
            {
                EmployeeFullName = evt.EmployeeFullName,
                ResignationDate = evt.ResignationDate.ToString("dd/MM/yyyy"),
                Reason = evt.Reason,
                CompanyName = evt.CompanyName   // ← ajouter
            }
        };

        await SendNotificationAsync(request);
    }
}