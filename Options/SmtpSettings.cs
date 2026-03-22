namespace ms_notification.Options;

public class SmtpSettings
{
    public string Server { get; set; } = null!;
    public int Port { get; set; }
    public string SenderName { get; set; } = "Woridge System";
    public string SenderEmail { get; set; } = null!;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public bool UseSsl { get; set; }
}