

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
}
    
