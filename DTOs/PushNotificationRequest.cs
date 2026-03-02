using ms_notification.Enum;
using System.ComponentModel.DataAnnotations;

namespace ms_notification.DTOs
{
    public class PushNotificationRequest
    {
        public string TargetCin { get; set; } = null!;
        public string TemplateName { get; set; }   //(ex: "Leave/LeaveApproved")
        [Required(ErrorMessage ="You must spesify a channel")]
        public Channel Channel { get; set; }      


        public Dictionary<string, string> Data { get; set; }
        [Required]
        public NotificationType NotificationType { get; set; }
        [Required]
        public Priority Priority { get; set; }
    }
}
