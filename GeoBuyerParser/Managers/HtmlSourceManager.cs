using PuppeteerSharp;

namespace GeoBuyerParser.Managers
{
    public static class HtmlSourceManager
    {
        public static async Task<string> DownloadHtmlSourceCode(string url)
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                string htmlSourceCode = await response.Content.ReadAsStringAsync();
                return htmlSourceCode;
            }
            else
            {
                return string.Empty;
            }
        }

        public static async Task<string> DownloadHtmlWithPuppeteerSharp(string url)
        {
            try
            {
                // Ensure PuppeteerSharp is initialized and browser revision is downloaded
                await new BrowserFetcher().DownloadAsync();

                Console.WriteLine($"Start {url}");

                using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true
                }))
                using (var page = await browser.NewPageAsync())
                {
                    await page.GoToAsync(url);
                    await page.WaitForTimeoutAsync(3000); // Adjust the timeout as needed

                    Console.WriteLine($"End {url}");

                    var html = await page.GetContentAsync();
                    return html ?? "";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DownloadHtmlWithPuppeteerSharp: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
