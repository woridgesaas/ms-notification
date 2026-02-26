using ms_notification.Enum;

namespace ms_notification.DTO
{
    public class SendNotificationRequest
    {
        public string TemplateName { get; set; }
        public Channel Channel { get; set; }
        public string ToEmail { get; set; }
        public Dictionary<string, string> Data { get; set; } = new();
    }
}
