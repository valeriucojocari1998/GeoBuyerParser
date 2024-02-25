using System.Net.Http;
using System.Net;

namespace GeoBuyerParser.Managers
{
    public static class HtmlSourceManager
    {
        public static async Task<string> DownloadHtmlSourceCode(string url, string? proxyAddress = "brd.superproxy.io:9222", string? proxyUsername = "brd-customer-hl_8b8a4690-zone-skvidy", string? proxyPassword = "48i5b920eagw")
        {
            try
            {
                // Create a WebProxy with the specified address
                var webProxy = new WebProxy(proxyAddress)
                {
                    // If your proxy requires authentication, set the credentials
                    Credentials = new NetworkCredential(proxyUsername, proxyPassword)
                };

                // Create a WebClient with the configured proxy
                using (var webClient = new WebClient())
                {
                    // Download the HTML content
                    string htmlSourceCode = await webClient.DownloadStringTaskAsync(url);
                    Thread.Sleep(100);
                    return htmlSourceCode;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DownloadHtmlSourceCodeWithProxy: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
