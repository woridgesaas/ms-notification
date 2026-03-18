namespace ms_notification.Options;

/// <summary>
/// Noms des queues RabbitMQ.
/// Extensible : ajouter UserUpdated, UserDeleted etc. sans toucher RabbitMQSettings.
/// </summary>
public sealed class RabbitMQQueues
{
    public string UserCreated { get; set; } = default!;
}
