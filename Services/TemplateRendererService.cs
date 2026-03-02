using ms_notification.Enum;
using RazorLight;
using System.Text.RegularExpressions;

namespace ms_notification.Services;

public class TemplateRendererService : ITemplateRendererService
{
    private readonly RazorLightEngine _razorLightEngine;
    private readonly string _templateRootPath;
    private static readonly Regex PlaceholderRegex = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public TemplateRendererService(IWebHostEnvironment env)
    {
        _templateRootPath = Path.Combine(env.ContentRootPath, "Templates");

        _razorLightEngine = new RazorLightEngineBuilder().UseFileSystemProject(_templateRootPath).UseMemoryCachingProvider().Build();
    }

    public async Task<string> RenderTemplateAsync(TemplateCode templateCode, object data)
    {
        string templateName = templateCode.ToString();
        string relativePath = $"Email/{templateName}.cshtml";
        string fullPath = Path.Combine(_templateRootPath, relativePath);

        if (!File.Exists(fullPath))
        {
            throw new TemplateNotFoundException("The following template : " + templateName + " was not found");
        }

        return await _razorLightEngine.CompileRenderAsync(relativePath, data);
    }
    
    public  string RenderTemplate(string templateContent, Dictionary<string, string> values)
    {
      
        var insensitiveValues = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);

        return PlaceholderRegex.Replace(templateContent, match =>
        {
            var key = match.Groups[1].Value;
            return insensitiveValues.TryGetValue(key, out var value) ? value : match.Value;
        });
    }

}