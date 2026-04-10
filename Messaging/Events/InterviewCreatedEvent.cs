namespace ms_notification.Messaging.Events;

public class InterviewCreatedEvent
{
    public int InterviewId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public string InterviewerName { get; set; } = string.Empty;
    public string InterviewerEmail { get; set; } = string.Empty;
    public string InterviewerCin { get; set; } = string.Empty;
    public string StartDateTime { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? VideoConferenceLink { get; set; }
    public string? Location { get; set; }
    public string? PhoneNumber { get; set; }
    public string TargetPosition { get; set; } = string.Empty;
}