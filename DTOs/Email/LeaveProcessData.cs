using ms_notification.Enum;

namespace ms_notification.DTOs.Email
{
    public class LeaveProcessData
    {
        public string? EmployeeFullName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public String LeaveType { get; set; }

        public LeaveRequestDecision DecisionStatus { get; set; }
        public DateTime DecisionDate { get; set; }
        public string? ApprovedByFullName { get; set; }

        public int RemainingBalance { get; set; }
    }
}
