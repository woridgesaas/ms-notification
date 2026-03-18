using System;

namespace ms_notification.Exceptions;

public class RabbitMQConsumerException(string message) : Exception(message)
{

}
