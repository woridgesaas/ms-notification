namespace ms_notification.Messaging.Events
{
    public class TenantReadyEvent
    {
        public int SubscriptionId { get; set; }
        public string Subdomain { get; set; } = string.Empty;
        public string ServiceKey { get; set; } = string.Empty;
    }
}
