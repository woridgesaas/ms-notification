using RabbitMQ.Client.Exceptions;

namespace ms_notification.Options;

/// <summary>
/// Routing keys RabbitMQ utilisées pour le binding queue ↔ exchange.
/// Extensible : ajouter UserUpdated, UserDeleted etc. sans toucher RabbitMQSettings.
/// </summary>
public sealed class RabbitMQRoutingKeys
{
    public string UserCreated { get; set; } = default!;
    public string ResignationCreated { get; set; } = default!;
    public string AbsenceAlertRh { get; set; } = null!; 
    public string AbsenceAlertCeo { get; set; } = null!;
    public string ChangePasswordOtp { get; set; } = default!;
    public string OvertimeProcessedAlert { get; set; } = null!;
    public string LeaveProcessedAlert { get; set; } = null!;
    public string DocumentRequestCreated { get; set; } = null!;
    public string UserSyncCreated { get; set; } = default!;

    //interviews
    public string InterviewCreated { get; set; } = default!;
    public string InterviewStatusChanged { get; set; } = default!;
    public string EvaluationSubmitted { get; set; } = default!;
}