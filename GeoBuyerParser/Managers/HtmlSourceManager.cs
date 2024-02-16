

using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

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
        var options = new EdgeOptions();
        options.AddArgument("--headless");

        using (var driver = new EdgeDriver(options))
        {
            driver.Navigate().GoToUrl(url);

            // Add a wait here if needed
            Thread.Sleep(TimeSpan.FromSeconds(2));

            var html = driver.PageSource;
            return html ?? "";
        }
    }
}
    
