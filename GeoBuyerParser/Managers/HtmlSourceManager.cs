

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace GeoBuyerParser.Managers;

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

    public static string DownloadHtmlWithSelenium(string url)
    {
        var options = new ChromeOptions();
        options.AddArguments("--headless"); // Run Chrome in headless mode.
        options.AddArguments("--no-sandbox"); // Bypass OS security model.
        options.AddArguments("--disable-dev-shm-usage"); // Overcome limited resource problems.
        options.AddArguments("--disable-gpu"); // Applicable for Windows only to enable headless mode.

        using (var driver = new ChromeDriver(options))
        {
            driver.Navigate().GoToUrl(url);

            // Add a wait here if needed
            Thread.Sleep(TimeSpan.FromSeconds(3));

            var html = driver.PageSource;
            return html ?? "";
        }
    }
}
    
