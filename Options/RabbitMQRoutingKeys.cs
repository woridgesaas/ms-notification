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
}