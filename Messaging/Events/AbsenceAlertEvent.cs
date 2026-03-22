namespace ms_notification.Messaging.Events;

public class AbsenceAlertEvent
{
    public string EmployeeFullName { get; set; } = null!;
    public string EmployeeEmail { get; set; } = null!;
    public string AbsenceDate { get; set; } = null!;
    public int ConsecutiveDays { get; set; }
    public string AlertLevel { get; set; } = null!;  // "RH" ou "CEO"
    public List<string> RecipientCins { get; set; } = [];
}