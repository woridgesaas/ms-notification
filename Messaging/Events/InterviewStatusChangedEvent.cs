namespace ms_notification.Messaging.Events;

public class InterviewStatusChangedEvent
{
    public int InterviewId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string InterviewerName { get; set; } = string.Empty;
    public string InterviewerEmail { get; set; } = string.Empty;
    public string InterviewerCin { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string StartDateTime { get; set; } = string.Empty;
}