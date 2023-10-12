using System.Globalization;
using System.Text.RegularExpressions;

namespace GeoBuyerParser.Helpers;

public record ParserHelper
{
    public static string RemoveMultipleSpaces(string? input)
    {
        if (input == null)
            return string.Empty;

        return Regex.Replace(input, @"\s+", " ");
    }


    public static decimal ParsePrice(string priceText)
    {
        decimal price;
        string cleanedText = priceText.Replace("\n", "").Replace("zł", "").Trim();
        cleanedText = RemoveMultipleSpaces(cleanedText);
        cleanedText = cleanedText.Replace(".", ",");
        cleanedText = cleanedText.Replace(" ", ".");
        decimal.TryParse(cleanedText, NumberStyles.Any, CultureInfo.InvariantCulture, out price);
        return price;
    }

    public static string NormalizeUrl(string url)
    {
        string decodedUrl = Uri.UnescapeDataString(url);
        return decodedUrl;
    }

    public static string RemoveNumberPart(string input)
    {
        // Define a regular expression pattern to match "(number)" and spaces around it.
        string pattern = @"\s+\(\d+\)\s+";

        // Replace the matched pattern with an empty string to remove it.
        string result = Regex.Replace(input, pattern, " ");
        return result.Trim(); // Trim to remove any leading/trailing spaces.
    }

    public static string ModifyImageUrl(string originalUrl)
    {
        try
        {
            originalUrl = originalUrl.Replace("thumbnailFixedWidth", "large");
            var urlList = originalUrl.Split('/');
            var lastPart = urlList.LastOrDefault();
            var newLast = lastPart?.Split("340").FirstOrDefault() + "1-1.jpg";
            var newUrlList = urlList.SkipLast(1).ToList();
            newUrlList.Add(newLast);
            var modifiedUrl = string.Join('/', newUrlList);

            return modifiedUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return originalUrl; // Return the original URL in case of an error
        }
    }

    public static string ChangeNumberInUrl(string originalUrl, int newFirstNumber)
    {
        try
        {
            UriBuilder uriBuilder = new UriBuilder(originalUrl);
            string path = uriBuilder.Path;

            // Find and replace the first part of the number in the URL (before the hyphen)
            int startIndex = path.LastIndexOf('-');
            int endIndex = path.LastIndexOf('.');

            if (startIndex != -1 && endIndex != -1)
            {
                string currentNumber = path.Substring(startIndex + 1, endIndex - startIndex - 1);
                string[] parts = currentNumber.Split('-');

                if (parts.Length == 2)
                {
                    // Replace the first part with the new first number
                    parts[0] = newFirstNumber.ToString();
                    string newNumberString = string.Join("-", parts);
                    path = path.Replace(currentNumber, newNumberString);
                }
            }

            // Update the modified path in the URI
            uriBuilder.Path = path;

            // Get the modified URL
            string modifiedUrl = uriBuilder.Uri.ToString();

            return modifiedUrl;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            return originalUrl; // Return the original URL in case of an error
        }
    }
}
