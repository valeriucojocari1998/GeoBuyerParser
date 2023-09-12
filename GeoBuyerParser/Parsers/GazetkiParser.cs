using GeoBuyerParser.Helpers;
using GeoBuyerParser.Models;
using HtmlAgilityPack;
using System.Net;

namespace GeoBuyerParser.Parsers;

public record GazetkiParser
{

    public List<Spot> GetSpots(string html)
    {
        try
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return GetSpotsInternal(doc);
        }
        catch (Exception ex)
        {
            // Handle the exception here.
            Console.WriteLine("Error in GetSpots: " + ex.Message);
            return new List<Spot>();
        }
    }

    public List<Spot> GetSpotsInternal(HtmlDocument document)
    {
        var spotNodes = document.DocumentNode.SelectNodes("//a[contains(@class, 'grid__row-item')]");
        if (spotNodes == null)
            return new List<Spot>();

        return spotNodes.Select(node =>
        {
            string provider = "";
            string url = "";
            string imageUrl = "";

            url = node.Attributes["href"].Value;
            // gets the node of the category
            var imageNode = node.SelectSingleNode(".//img[contains(@class, 'v-lazy-image-loaded')]");
            var providerNode = node.SelectSingleNode(".//h3[contains(@class, 'store-name')]");

            if (imageNode != null)
            {
                imageUrl = imageNode.Attributes["src"].Value;
            }

            if (providerNode != null)
            {
                provider = ParserHelper.RemoveMultipleSpaces(providerNode.InnerHtml);
            }

            return new Spot(id: Guid.NewGuid().ToString(), provider: provider, imageUrl: imageUrl, url: url);
        }).ToList();
    }

    public List<Newspaper> GetNewspapers(string html, string spotId)
    {
        try
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var newsPaperNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'store-flyer mb-3')]");
            if (newsPaperNodes == null)
                return new List<Newspaper>();

            return newsPaperNodes.Select(node =>
            {
                try
                {
                    string id = new Guid().ToString();
                    string name = "";
                    string newspaperCode = "";
                    string url = "";
                    string imageUrl = "";
                    string? validInfo = null;
                    string? type = null;
                    string? description = null;

                    var infoNode = node.SelectSingleNode(".//div[contains(@class, 'store-flyer__info')]");
                    var urlNode = node.SelectSingleNode(".//a[contains(@class, 'store-flyer__image')]");
                    if (urlNode != null)
                    {
                        url = urlNode.Attributes["href"].Value;
                        newspaperCode = url.Split('-').Last();
                        var imageNode = urlNode.SelectSingleNode(".//img[contains(@class, 'v-lazy-image-loaded')]");
                        if (imageNode != null)
                            imageUrl = imageNode?.Attributes["src"].Value;
                    }
                    if (infoNode != null)
                    {
                        var nameNode = infoNode.SelectSingleNode(".//h3");
                        if (nameNode != null)
                            name = ParserHelper.RemoveMultipleSpaces(nameNode.InnerText);
                        var validInfoNode = infoNode.SelectSingleNode(".//small");
                        if (validInfoNode != null)
                            validInfo = ParserHelper.RemoveMultipleSpaces(validInfoNode.InnerText);
                        var typeNode = infoNode.SelectSingleNode(".//div[contains(@class, 'store-flyer__trend')]");
                        if (typeNode != null)
                            type = ParserHelper.RemoveMultipleSpaces(typeNode.InnerText);
                        var descriptionNode = infoNode.SelectSingleNode(".//p");
                        if (descriptionNode != null)
                            description = ParserHelper.RemoveMultipleSpaces(descriptionNode.InnerText);
                    }

                    return new Newspaper(id: Guid.NewGuid().ToString(), name: name, newspaperCode: newspaperCode, spotId: spotId, url: url, imageUrl: imageUrl, validInfo: validInfo, type: type, description: description);
                }
                catch (Exception ex)
                {
                    // Handle the exception for this specific node.
                    Console.WriteLine("Error in GetNewspapers: " + ex.Message);
                    return null; // Skip this newspaper or handle the error as needed.
                }
            }).ToList().Where(x => x != null).ToList();
        }
        catch (Exception ex)
        {
            // Handle the exception here.
            Console.WriteLine("Error in GetNewspapers: " + ex.Message);
            return new List<Newspaper>();
        }
    }

    public int GetNewspaperPagesCount(string html)
    {
        HtmlDocument document = new HtmlDocument();
        document.LoadHtml(html);
        var newsPaperNodes = document.DocumentNode.SelectNodes("//div[contains(@class, 'zoomer')]");
        if (newsPaperNodes == null)
            return 0;
        return newsPaperNodes.Count;
    }

    public async Task<(NewspaperPage page, List<ExtendedProduct>? products)> GetNewspaperPage(string html, string pageNumber, string newspaperId, string url, Spot spot)
    {
        try
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var newsPaperNode = document.DocumentNode.SelectSingleNode($"//*[@id='zoomer{pageNumber}']");
            if (newsPaperNode == null)
                return (null, null);
            var imageNode = newsPaperNode.SelectSingleNode(".//img");
            var newPage = new NewspaperPage(id: new Guid().ToString(), page: pageNumber, newspaperId: newspaperId, pageUrl: url, imageUrl: imageNode?.Attributes["src"].Value);
            var productNodes = newsPaperNode.SelectNodes(".//div[contains(@class, 'markerIconHolder')]");
            var productCodes = productNodes.Select(x => x.GetAttributeValue("onclick", "").Split('(').Last().Split(')').First());
            var productTaks = productCodes.Select(async x => await DownloadProductInfoByCode(x));

            var products = (await Task.WhenAll(productTaks)).Select(x => new ExtendedProduct(x, spot)).ToList();
            return (newPage, products);
        } catch {
            return (null, null);
        }
    }

    public async Task<Product> DownloadProductInfoByCode(string productcode) {
        var products = new List<Product>();
        try
        {

        using var handler = new HttpClientHandler();
        handler.AutomaticDecompression = ~DecompressionMethods.None;
        using var httpClient = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, $"https://www.gazetki.pl/related-offers-dynamic/{productcode}/1");

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();

            var parsedData = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(responseContent);

            if (parsedData != null)
            {
                // Convert the items to the Product type
                foreach (var item in parsedData.items)
                {
                    var product = new Product(
                        // id: item.id.ToString(),
                        id: Guid.NewGuid().ToString(),
                        name: item.name,
                        currentPrice: (decimal)(item.offer_price_flat ?? 0),
                        oldPrice: (decimal)(item.normal_price_flat ?? 0),
                        brand: item.store?.is_brand == true ? item.store?.name : null,
                        priceLabel: item.sub_title,
                        saleSpecification: item.end_date_template,
                        imageUrl: item.image_tn?.url
                    );

                    products.Add(product);
                }
            }

        }

        }
        catch (Exception ex) { }

        return products?.Count > 0 ? products.First() : null;
    }

    public int GetProductCount(string html)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);
        var navNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'top-header__nav hidden-scrollbar')]");
        if (navNode == null)
            return 0;

        var linkNodes = navNode.SelectNodes("./a");
        if (linkNodes == null)
            return 0;

        var offertNodes = linkNodes.FirstOrDefault(x => x.Attributes["href"]?.Value.Contains("oferty") ?? false);
        if (offertNodes == null)
            return 0;

        var totalText = offertNodes.SelectSingleNode("./span[contains(@class, 'ml-1 badge rounded-pill bg-tertiary')]")?.InnerText;
        return int.TryParse(totalText, out var x) ? x : 0;
    }

    public async Task<List<Product>> GetProducts(string url, int total, string csrf)
    {
        var products = new List<Product>();
        var page = 1;

        while ((page - 1) * 30 < total)
        {
            products.AddRange(await GetProductsInternal(url, page, csrf));
            page++;
        }

        return products;
    }

    public async Task<List<Product>> GetProductsInternal(string url, int page, string csrf)
    {
        var products = new List<Product>();

        try
        {
            using var handler = new HttpClientHandler();
            handler.AutomaticDecompression = ~DecompressionMethods.None;

            using var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{url}offers/dynamic/{page}");

            request.Headers.TryAddWithoutValidation("x-csrf", csrf);

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var parsedData = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(responseContent);

                if (parsedData != null)
                {
                    // Convert the items to the Product type
                    foreach (var item in parsedData.items)
                    {
                        var product = new Product(
                            //id: item.id.ToString(),
                            id: Guid.NewGuid().ToString(),
                            name: item.name,
                            currentPrice: (decimal)(item.offer_price_flat ?? 0),
                            oldPrice: (decimal)(item.normal_price_flat ?? 0),
                            brand: item.store?.is_brand == true ? item.store?.name : null,
                            priceLabel: item.sub_title,
                            saleSpecification: item.end_date_template,
                            imageUrl: item.image_tn?.url
                        );

                        products.Add(product);
                    }
                }

            }
            else
            {
                Console.WriteLine($"Request failed with status code: {response.StatusCode}");
            }
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }

        

        return products;
    }
}

public class Image
{
    public string _type { get; set; }
    public string url { get; set; }
}

public class CategoryRef
{
    public string _type { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public string icon_alias { get; set; }
    public bool spotlight { get; set; }
}

public class StoreRef
{
    public string _type { get; set; }
    public string _class { get; set; }
    public int id { get; set; }
    public string slug { get; set; }
    public string country { get; set; }
    public string name { get; set; }
    public Image logo_tn { get; set; }
    public List<CategoryRef> categories { get; set; }
    public bool spotlight { get; set; }
    public bool popular { get; set; }
    public bool is_brand { get; set; }
    public bool online { get; set; }
    public object feed { get; set; }
    public string website { get; set; }
}

public class Item
{
    public string _type { get; set; }
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public string path { get; set; }
    public string normal_price { get; set; }
    public string end_date_template { get; set; }
    public string offer_price { get; set; }
    public string sub_title { get; set; }
    public Image image_tn { get; set; }
    public string image_alt_text { get; set; }
    public StoreRef store { get; set; }
    public bool valid_in_future { get; set; }
    public double? normal_price_flat { get; set; }
    public double? offer_price_flat { get; set; }
    public object votes_total { get; set; }
}

public class ShoppingListInfo
{
    public bool is_user { get; set; }
    public string change_item_from_offer_url { get; set; }
}

public class Root
{
    public string _type { get; set; }
    public object total { get; set; }
    public int offset { get; set; }
    public int page_size { get; set; }
    public List<Item> items { get; set; }
    public ShoppingListInfo shopping_list_info { get; set; }
}