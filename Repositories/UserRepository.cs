using Microsoft.EntityFrameworkCore;
using ms_notification.Data;
using ms_notification.Models;

namespace ms_notification.Services;

public class UserRepository(NotificationDbContext context) : IUserRepository
{
    private readonly NotificationDbContext _context = context;

    public async Task<User?> GetUserByCINAsync(string CIN)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.CIN == CIN);
    }
}
