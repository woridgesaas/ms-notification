using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ms_notification.Hubs
{
    [Authorize(AuthenticationSchemes = "HeaderAuthentication")]
    public class NotificationHub: Hub
    {
        private ILogger<NotificationHub> __logger;
        public NotificationHub(ILogger<NotificationHub> lg) {
            this.__logger = lg;
         }
        public override async Task OnConnectedAsync()
        {
           
            // 1. Get Cin  claim from token
            var cin = Context.User?.FindFirst("CIN")?.Value;

            // 2. Get tenantSubdomain
            var tenantId = Context.User?.FindFirst("Subdomain")?.Value;

            await Groups.AddToGroupAsync(Context.ConnectionId, tenantId ?? "default");
            this.__logger.LogInformation("User with CIN={CIN} connected to Hub on group {TenantId}", cin, tenantId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            this.__logger.LogInformation("Connection {ConnectionId} disconnected from Hub. Reason: {Reason}", Context.ConnectionId, exception?.Message ?? "Client disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
