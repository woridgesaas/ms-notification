using ms_notification.Models;

namespace ms_notification.Services;

public interface IUserRepository
{
    Task<User?> GetUserByCINAsync(string CIN);
}
