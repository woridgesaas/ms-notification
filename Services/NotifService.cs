using ms_notification.DTO;
using ms_notification.Exceptions;
using ms_notification.Mappers;
using ms_notification.Models;
using ms_notification.Repositories;

namespace ms_notification.Services;

public class NotifService: INotifService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly INotificationRepository _notificationRepository;

    public NotifService(
        IEmployeeRepository employeeRepository,
        INotificationRepository notificationRepository)
    {
        _employeeRepository = employeeRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task CreateNotificationAsync(CreateNotificationRequest dto, string cin)
    {
        var employee = await _employeeRepository.GetByCinAsync(cin);

        if (employee == null)
            throw new NotFoundException($"Employee with CIN {cin} not found");
        var notification = NotificationMapper.ToEntity(dto, employee.Id);

        await _notificationRepository.AddNotificationAsync(notification);

        var receive = new NotificationReceive
        {
            EmployeeId = employee.Id,
            NotificationId = notification.Id,
            IsRead = false
        };

        await _notificationRepository.AddReceiveAsync(receive);
    }

    public async Task<List<NotificationResponse>> GetMyNotificationsAsync(string cin)
    {
        var employee = await _employeeRepository.GetByCinAsync(cin);

        if (employee == null)
            throw new NotFoundException($"Employee with CIN {cin} not found");

        var receives = await _notificationRepository.GetByEmployeeIdAsync(employee.Id);

        return NotificationMapper.ToResponseList(receives);
    }
    public async Task<bool> MarkAsReadAsync(int notificationId, string cin)
    {
        var employee = await _employeeRepository.GetByCinAsync(cin);
        if (employee == null)
            throw new NotFoundException($"Employee with CIN {cin} not found");

        var receive = await _notificationRepository
            .GetReceiveAsync(notificationId, employee.Id);

        if (receive == null)
            throw new NotFoundException($"Employee with CIN {cin} not found");

        receive.IsRead = true;
        receive.ReadDate = DateTime.UtcNow;

        await _notificationRepository.UpdateReceiveAsync(receive);
        return true;
    }

    public async Task MarkAllAsReadAsync(string cin)
    {
        var employee = await _employeeRepository.GetByCinAsync(cin);
        if (employee == null)
            throw new NotFoundException($"Employee with CIN {cin} not found");

        var receives = await _notificationRepository
            .GetUnreadByEmployeeIdAsync(employee.Id);

        foreach (var r in receives)
        {
            r.IsRead = true;
            r.ReadDate = DateTime.UtcNow;
        }

        await _notificationRepository.SaveChangesAsync();
    }
}