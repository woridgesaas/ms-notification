namespace ms_notification.DTOs.Events;

public class ResignationCreatedEvent
{
    public string ResignationId { get; set; }
    public string EmployeeEmail { get; set; } = null!;
    public string EmployeeFullName { get; set; } = null!;
    public string EmployeeCin { get; set; } = null!;
    public DateTime ResignationDate { get; set; }
    public string Reason { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
}