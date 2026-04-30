namespace ms_notification.Messaging.Events;

public sealed class ContractGeneratedEvent
{
    public int      JobApplicationId { get; set; }
    public string   CandidateName    { get; set; } = string.Empty;
    public string   CandidateEmail   { get; set; } = string.Empty;
    public string   ContractHtml     { get; set; } = string.Empty;
    public DateTime GeneratedAt      { get; set; }
}