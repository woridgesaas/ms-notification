using Microsoft.EntityFrameworkCore;
using ms_notification.Models;
using ms_notification.Persistence;

namespace ms_notification.Repositories;

public class NotificationRepository: INotificationRepository
{
    private readonly NotificationDbContext _context;

    public NotificationRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task AddNotificationAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task AddReceiveAsync(NotificationReceive receive)
    {
        await _context.NotificationReceives.AddAsync(receive);
        await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationReceive>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.NotificationReceives
            .Where(r => r.EmployeeId == employeeId)
            .Include(r => r.Notification)
            .OrderByDescending(r => r.Notification.CreationDate)
            .ToListAsync();
    }

    public async Task<NotificationReceive?> GetReceiveAsync(
    int notificationId, int employeeId)
    {
        return await _context.NotificationReceives
            .FirstOrDefaultAsync(r =>
                r.NotificationId == notificationId &&
                r.EmployeeId == employeeId);
    }

    public async Task<List<NotificationReceive>> GetUnreadByEmployeeIdAsync(
        int employeeId)
    {
        return await _context.NotificationReceives
            .Where(r => r.EmployeeId == employeeId && !r.IsRead)
            .ToListAsync();
    }

    public async Task UpdateReceiveAsync(NotificationReceive receive)
    {
        _context.NotificationReceives.Update(receive);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}