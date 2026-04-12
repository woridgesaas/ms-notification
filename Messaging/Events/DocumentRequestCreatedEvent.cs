namespace ms_notification.Messaging.Events
{
    public class DocumentRequestCreatedEvent
    {
        public string RequestId { get; set; }
        public string EmployeeCin { get; set; }
        public string EmployeeFullName { get; set; }
        public string DocumentCode { get; set; }
        public string DocumentGroup { get; set; }
        public DateTime RequestDate { get; set; }
    }
}