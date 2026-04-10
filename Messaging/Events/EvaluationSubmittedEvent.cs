namespace ms_notification.Messaging.Events;

public class EvaluationSubmittedEvent
{
    public int InterviewId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string InterviewerName { get; set; } = string.Empty;
    public string HrCin { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
}