using Microsoft.EntityFrameworkCore;
using ms_notification.Data;
using ms_notification.Models;

namespace ms_notification.Repositories;

public class NotificationRepository(NotificationDbContext context) : INotificationRepository
{
    private readonly NotificationDbContext _context = context;

    public async Task AddNotificationAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<List<NotificationReceive>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.NotificationReceives
            .Where(r => r.UserId == employeeId)
            .Include(r => r.Notification)
            .OrderByDescending(r => r.Notification.CreatedAt)
            .ToListAsync();
    }

    public async Task<NotificationReceive?> GetReceiveAsync(
    int notificationId, int employeeId)
    {
        return await _context.NotificationReceives
            .FirstOrDefaultAsync(r =>
                r.NotificationId == notificationId &&
                r.UserId == employeeId);
    }

    public async Task<List<NotificationReceive>> GetUnreadByEmployeeIdAsync(
        int employeeId)
    {
        return await _context.NotificationReceives
            .Where(r => r.UserId == employeeId && !r.IsRead)
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