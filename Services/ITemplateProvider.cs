using ms_notification.Enum;

namespace ms_notification.Services
{
    public interface ITemplateProvider
    {
        public  Task<string> GetTemplateAsync(string templateName, Channel channel);
    }
}
