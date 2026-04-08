using ms_notification.Enum;

namespace ms_notification.Messaging.Events
{
    public class OvertimeHandleEvent : AbstractEvent
    {
        public string ?ManagerComment { get; set; }
        public double TotalHours { get; set; }
        public DateTime ProcessedAt { get;set; }
        public OvertimeStatus Status { get; set; }
        public DateOnly WorkingDay { get; set; } //Date just

    }
}
