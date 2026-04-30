namespace ms_notification.Services;

public interface IPdfGeneratorService
{
    Task<byte[]> GenerateFromHtmlAsync(string html);
}