namespace ms_notification.Messaging.Events
{
    public class ChangePasswordOtpRequestedEvent
    {
        public string Email { get; set; } = default!;
        public string Cin { get; set; } = default!;
        public string OtpCode { get; set; } = default!;
    }
}
