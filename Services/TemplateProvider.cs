using ms_notification.Enum;
using ms_notification.Exceptions;

namespace ms_notification.Services
{
    public class TemplateProvider :ITemplateProvider
    {
        private readonly IWebHostEnvironment _env;

        public TemplateProvider(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> GetTemplateAsync(string templateName, Channel channel)
        {
            var folder = channel.ToString().Equals("Email", StringComparison.OrdinalIgnoreCase) ? "Email" : "App";
            var baseDir = Path.Combine(_env.ContentRootPath, "Templates", folder);
            string finalTemplate;
            var cshtmlPath = Path.Combine(baseDir, $"{templateName}.cshtml");
            var htmlPath = Path.Combine(baseDir, $"{templateName}.html");
            if (File.Exists(cshtmlPath))
            {
                finalTemplate = cshtmlPath;
            }            
            else if (File.Exists(htmlPath))
            {
                finalTemplate = htmlPath;
            }
            else
            {
                throw new TemplateNotFoundException($"Le template {templateName} n'existe pas en .cshtml ou .html dans {folder}");
            }
            return await File.ReadAllTextAsync(finalTemplate);

            
        }

    }
}
