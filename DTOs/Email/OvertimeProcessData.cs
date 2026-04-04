using ms_notification.Enum;

namespace ms_notification.DTOs.Email
{
    public class OvertimeProcessData
    {
        public string ? EmployeeFullName { get; set; }
        public string? Raison { get; set; }
        public double TotalHours { get; set; }
        public DateTime ProcessedAt { get; set; }
        public OvertimeStatus Status { get; set; }
        public DateOnly WorkingDay { get; set; } //Date just
    }
}
