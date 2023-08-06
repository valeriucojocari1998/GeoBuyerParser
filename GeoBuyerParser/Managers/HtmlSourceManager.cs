

namespace GeoBuyerPromotion.Managers
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
                Console.WriteLine("Failed to download the HTML source code. Status code: " + response.StatusCode);
                return string.Empty;
            }
        }
    }
        
}
