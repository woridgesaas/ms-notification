using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace ms_notification.Services;

public sealed class PdfGeneratorService : IPdfGeneratorService
{
    public async Task<byte[]> GenerateFromHtmlAsync(string html)
    {
        // Télécharge Chromium si pas déjà présent (première fois seulement)
        await new BrowserFetcher().DownloadAsync();

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        await using var page = await browser.NewPageAsync();
    await page.SetContentAsync(html);

        return await page.PdfDataAsync(new PdfOptions
        {
            Format        = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top    = "20mm",
                Bottom = "20mm",
                Left   = "15mm",
                Right  = "15mm"
            }
        });
    }
}