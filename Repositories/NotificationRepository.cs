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
}
