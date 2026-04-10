namespace ms_notification.DTOs.Email;

public class InterviewScheduledData
{
    public string CandidateName { get; set; } = string.Empty;
    public string InterviewerName { get; set; } = string.Empty;
    public string StartDateTime { get; set; } = string.Empty;
    public string DurationMinutes { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? VideoConferenceLink { get; set; }
    public string? Location { get; set; }
    public string? PhoneNumber { get; set; }
    public string TargetPosition { get; set; } = string.Empty;
    public string CompanyName { get; set; } = "Woridge";
}

public class InterviewAssignedData
{
    public string InterviewerName { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string StartDateTime { get; set; } = string.Empty;
    public string DurationMinutes { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? VideoConferenceLink { get; set; }
    public string? Location { get; set; }
    public string? PhoneNumber { get; set; }
    public string TargetPosition { get; set; } = string.Empty;
    public string CompanyName { get; set; } = "Woridge";
}