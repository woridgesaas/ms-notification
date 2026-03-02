using Microsoft.AspNetCore.SignalR;

namespace ms_notification.Config
{
    public class CinUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst("CIN")?.Value;
        }
    }
}
