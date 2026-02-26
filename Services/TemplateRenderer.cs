using System.Text.RegularExpressions;

namespace ms_notification.Services;

public class TemplateRenderer
{
    private static readonly Regex PlaceholderRegex =
        new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);

    public string Render(string template, Dictionary<string, string> values)
    {
        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups[1].Value;

            return values.TryGetValue(key, out var value)
                ? value
                : match.Value; // si variable absente on laisse {{variable}}
        });
    }
}