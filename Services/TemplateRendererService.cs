using ms_notification.Enum;
using RazorLight;

namespace ms_notification.Services;

public class TemplateRendererService : ITemplateRendererService
{
    private readonly RazorLightEngine _razorLightEngine;
    private readonly string _templateRootPath;

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
}