namespace ms_notification.DTOs.Email;

public class ResignationData
{
    public string EmployeeFullName { get; set; } = null!;  // remplace @Model.EmployeeFullName
    public string ResignationDate { get; set; } = null!;  // remplace @Model.ResignationDate
    public string Reason { get; set; } = null!;  // remplace @Model.Reason
    public string CompanyName { get; set; } = null!;
}