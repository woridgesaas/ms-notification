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

            // Cherche récursivement dans tous les sous-dossiers
            var files = Directory.GetFiles(baseDir, $"{templateName}.*", SearchOption.AllDirectories);

            var file = files.FirstOrDefault(f =>
                f.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(".html", StringComparison.OrdinalIgnoreCase));

            if (file == null)
                throw new TemplateNotFoundException($"Le template {templateName} n'existe pas en .cshtml ou .html dans {folder}");

            return await File.ReadAllTextAsync(file);
        }

    }
}
