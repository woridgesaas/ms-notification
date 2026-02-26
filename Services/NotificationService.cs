using ms_notification.Services;

namespace ms_notification.Services;

public class NotificationService
{
    private readonly TemplateProvider _templateProvider;
    private readonly TemplateRenderer _templateRenderer;
    private readonly EmailService _emailService;

    public NotificationService(
        TemplateProvider templateProvider,
        TemplateRenderer templateRenderer,
        EmailService emailService)
    {
        _templateProvider = templateProvider;
        _templateRenderer = templateRenderer;
        _emailService = emailService;
    }

    public async Task SendAsync(
        string templateName,
        string channel,
        string toEmail,
        Dictionary<string, string> data)
    {
        var template = await _templateProvider.GetTemplateAsync(templateName, channel);

        var body = _templateRenderer.Render(template, data);

        if (channel.Equals("Email", StringComparison.OrdinalIgnoreCase))
        {
            await _emailService.SendEmailAsync(toEmail, templateName, body);
        }

        // Pour App notification tu pourras ajouter SignalR plus tard
    }
}