using ms_notification.Enum;

namespace ms_notification.Services;

public interface ITemplateRendererService
{
    Task<string> RenderTemplateAsync(TemplateCode templateCode,  object data);
    string RenderTemplate(string templateCode, Dictionary<string, string>values);

}
