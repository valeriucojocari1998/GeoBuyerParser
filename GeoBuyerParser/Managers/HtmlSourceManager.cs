

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
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl("https://bstackdemo.com/");

        Thread.Sleep(TimeSpan.FromSeconds(3));
        var html = driver.PageSource;
        driver.Quit();

        return html ?? "";
    }
}
    
