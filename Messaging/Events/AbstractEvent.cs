namespace ms_notification.Messaging.Events
{
    public abstract class AbstractEvent
    {
        public string? EmployeeCin { get; set; }
        public string? EmployeeEmail { get; set; }

        public string? EmployeeFullName { get; set; }
    }
}
