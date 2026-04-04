using ms_notification.DTOs.Email;
using ms_notification.Enum;
using ms_notification.Messaging.Events;

namespace ms_notification.Factories
{
    public class NotificationRequestFactory : INotificationRequestFactory
    {
        public SendNotificationRequest<OvertimeProcessData> CreateOvertimeRequest(OvertimeHandleEvent ev)
        {
            return new SendNotificationRequest<OvertimeProcessData>
            {
                To = ev.EmployeeEmail,
                CreatedBy = ev.EmployeeCin,
                Channel = Channel.EMAIL,
                TemplateCode = TemplateCode.OVERTIME_ALERT_EMPLOYEE,
                Data = new OvertimeProcessData
                {
                    EmployeeFullName = ev.EmployeeFullName,
                    Raison = ev.ManagerComment,
                    WorkingDay = ev.WorkingDay,
                    ProcessedAt = ev.ProcessedAt,
                    Status = ev.Status,
                    TotalHours = ev.TotalHours
                }
            };
        }


      
    }
}
