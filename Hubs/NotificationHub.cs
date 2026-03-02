using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ms_notification.Hubs
{
    [Authorize]
    public class NotificationHub: Hub
    {
        public override async Task OnConnectedAsync()
        {
            // 1. Get Cin  claim from token
            var cin = Context.User?.FindFirst("CIN")?.Value;

            // 2. Get tenantSubdomain camme from gateway , HARDCODED actually
            string tenantId = "group1";

            await Groups.AddToGroupAsync(Context.ConnectionId, tenantId);
            Console.WriteLine($"User {cin} Connected To group {tenantId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("User has been disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
