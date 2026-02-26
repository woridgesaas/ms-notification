using Microsoft.AspNetCore.Hosting;

namespace ms_notification.Services;

public class TemplateProvider
{
    private readonly IWebHostEnvironment _env;

    public TemplateProvider(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> GetTemplateAsync(string templateName, string channel)
    {
        var folder = channel.Equals("Email", StringComparison.OrdinalIgnoreCase)
            ? "Email"
            : "App";
        var path = Path.Combine(
            _env.ContentRootPath,
            "Templates",
            folder,
            $"{templateName}.html");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Template not found: {templateName}");

        return await File.ReadAllTextAsync(path);
    }
}