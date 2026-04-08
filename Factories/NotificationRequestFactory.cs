using ms_notification.DTOs.Email;
using ms_notification.Enum;
using ms_notification.Messaging.Events;

namespace ms_notification.Factories
{
    public class NotificationRequestFactory : INotificationRequestFactory
    {

        public SendNotificationRequest<TData> CreateRequest<TData>(AbstractEvent ev, TemplateCode templateCode)where TData : class
        {
            switch (ev)
            {
                case OvertimeHandleEvent overtime:
                    var overtimeData = new OvertimeProcessData
                    {
                        EmployeeFullName = overtime.EmployeeFullName,
                        Raison = overtime.ManagerComment,
                        WorkingDay = overtime.WorkingDay,
                        ProcessedAt = overtime.ProcessedAt,
                        Status = overtime.Status,
                        TotalHours = overtime.TotalHours
                    };
                    return (SendNotificationRequest<TData>)(object) new SendNotificationRequest<OvertimeProcessData>
                    {
                        To = overtime.EmployeeEmail,
                        CreatedBy = overtime.EmployeeCin,
                        Channel = Channel.EMAIL,
                        TemplateCode = templateCode,
                        Data = overtimeData
                    };

                case LeaveHandleEvent leave:
                    var leaveData = new LeaveProcessData
                    {
                        EmployeeFullName = leave.EmployeeFullName,
                        StartDate = leave.StartDate,
                        EndDate = leave.EndDate,
                        LeaveType = leave.LeaveType,
                        DecisionStatus = leave.DecisionStatus,
                        DecisionDate = leave.DecisionDate,
                        ApprovedByFullName = leave.ApprovedByFullName,
                        RemainingBalance = leave.RemainingBalance
                    };
                    return (SendNotificationRequest<TData>)(object)new SendNotificationRequest<LeaveProcessData>
                    {
                        To = leave.EmployeeEmail,
                        CreatedBy = leave.EmployeeCin,
                        Channel = Channel.EMAIL,
                        TemplateCode = templateCode,
                        Data = leaveData
                    };

                default:
                    throw new NotSupportedException($"Event type {ev.GetType().Name} not supported for {typeof(TData).Name}");
            }


        }
    }
}
