using ms_notification.Enum;

namespace ms_notification.Messaging.Events
{
    public class LeaveHandleEvent : AbstractEvent
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public String LeaveType { get; set; }

        public LeaveRequestDecision DecisionStatus { get; set; }
        public DateTime DecisionDate { get; set; }
        public string ?ApprovedByFullName { get; set; }

        public int RemainingBalance { get; set; }
    }
}
