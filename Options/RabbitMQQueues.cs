namespace ms_notification.Options;

/// <summary>
/// Noms des queues RabbitMQ.
/// Extensible : ajouter UserUpdated, UserDeleted etc. sans toucher RabbitMQSettings.
/// </summary>
public sealed class RabbitMQQueues
{
    public string UserCreated { get; set; } = default!;
    public string ResignationCreated { get; set; } = default!;
    public string AbsenceAlertRh { get; set; } = null!;  
    public string AbsenceAlertCeo { get; set; } = null!;
    public string ChangePasswordOtp { get; set; } = default!;
    public string OvertimeHandleAlert { get; set; } = null!;
    public string LeaveHandleAlert { get; set; } = null!;
    public string DocumentRequestCreated { get; set; } = null!;
    public string UserSyncCreated { get; set; } = default!;
    public string ContractGenerated { get; set; } = string.Empty;


    //interviws
    public string InterviewCreated { get; set; } = default!;
    public string InterviewStatusChanged { get; set; } = default!;
    public string EvaluationSubmitted { get; set; } = default!;
}
