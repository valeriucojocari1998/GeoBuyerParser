using GeoBuyerParser.Helpers;
using GeoBuyerParser.Managers;
using GeoBuyerParser.Models;
using GeoBuyerParser.Parsers;
using GeoBuyerParser.Repositories;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace GeoBuyerParser.Services;

public record GazetkiService
{
    public Repository Repository { get; }
    public GazetkiParser Parser { get; }

    public ProductService ProductService { get; }

    public string BaseUrl { get; } = "https://www.gazetki.pl";
    public string ShopsUrl { get; } = "https://www.gazetki.pl/sklepy/";
    public List<string> ShopsQualifiers { get; } = new List<string>() { "0-9", "a", "a/2", "b", "b/2", "c", "c/2", "d", "d/2", "e", "f", "g", "h", "i", "j", "k", "k/2", "l", "m", "m/2", "n", "o", "p", "p/2", "q", "r", "s", "s/2", "s/3", "t", "t/2", "u", "v", "w", "x", "y", "z" };

    public GazetkiService(Repository repository, GazetkiParser parser, ProductService productService)
    {
        Repository = repository;
        Parser = parser;
        ProductService = productService;
    }

    public async Task<string> GetCSRF()
    {

        var html = await HtmlSourceManager.DownloadHtmlWithPuppeteerSharp(BaseUrl);
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        var scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(., 'window.csrf')]");
        if (scriptNode == null)
            throw new Exception("CSRF not parsed");

        string scriptContent = scriptNode.InnerHtml;
        string pattern = @"window\.csrf\s*=\s*""([^""]+)"";";
        var match = Regex.Match(scriptContent, pattern);
        return match.Groups[1].Value;
    }

    public async Task<int> GetProducts()
    {
        try
        {
            var csfr = await GetCSRF();
            var products = new Dictionary<string, ExtendedProduct>();
            var spots = new List<Spot>();
            var newspapers = new List<Newspaper>();
            var pages = new List<NewspaperPage>();

            // all data
            var data = await GetProductsInternal(csfr);
            data.products.ForEach(x => products.Add(x.id, x));
            spots.AddRange(data.spots);
            // by newsppaers
            var newsPapersData = await GetNewspapaersInternal(data.spots);
            newspapers.AddRange(newsPapersData.newspapers);
            pages.AddRange(newsPapersData.pages);
            newsPapersData.products.ForEach(x => products.Add(x.id, x));
            var valuesToAdd = products.Select(x => x.Value).ToList();
            ProductService.AddOrOverrideProducts(valuesToAdd);
            Repository.InsertNewspapers(newspapers);
            Repository.InserNewspapersPages(pages);
            Repository.InsertSpots(spots);

            return products.Count;
        }
        catch (Exception ex)
        {
            // Handle the exception here.
            Console.WriteLine("Error in GetProducts: " + ex.Message);
            return 0; // Return an empty list or handle the error as needed.
        }
    }

    public async Task<(List<Newspaper> newspapers, List<NewspaperPage> pages, List<ExtendedProduct> products)> GetNewspapaersInternal(List<Spot> spots)
    {
        try
        {
            var tasks = spots.Select(async spot =>
            {
                try
                {
                    var url = BaseUrl + spot.url;
                    var html = await HtmlSourceManager.DownloadHtmlWithPuppeteerSharp(url);
                    var spotNews = Parser.GetNewspapers(html, spot.id);
                    return spotNews;
                }
                catch (Exception ex)
                {
                    // Handle the exception for this specific task.
                    Console.WriteLine("Error in task: " + ex.Message);
                    return new List<Newspaper>(); // Return an empty list or handle the error as needed.
                }
            });

            var newsPaperLists = await Task.WhenAll(tasks);
            var newspapers = newsPaperLists.SelectMany(list => list).ToList();
            var newNespapers = newspapers.Where(x => x.imageUrl.Contains("thumbnailFixedWidth")).Select(x => x.ChangeImageUrl(ParserHelper.ModifyImageUrl(x.imageUrl))).ToList();

            var pageTasks = newNespapers.Select(async paper =>
            {
                try
                {
                    var spot = spots.First(x => x.id == paper.spotId);
                    var html = await HtmlSourceManager.DownloadHtmlWithPuppeteerSharp(BaseUrl + paper.url + "#page=1");
                    var (newPages, newProducts) = GetNewspapersAndProducts(html, spot, paper.id);
                    var newnewProducts = newProducts.Select(x => new ExtendedProduct(x, spot));
                    return (pages: newPages, products: newnewProducts);
                }
                catch (Exception ex)
                {
                    // Handle the exception for this specific task.
                    Console.WriteLine("Error in task: " + ex.Message);
                    return (pages: new List<NewspaperPage>(), products: new List<ExtendedProduct>());
                }
            });

            var newsPaperPagesLists = await Task.WhenAll(pageTasks);
            var newsPaperPages = newsPaperPagesLists.Select(list => list.pages).SelectMany(list => list).ToList();
            var products = newsPaperPagesLists.Select(list => list.products).SelectMany(list => list).ToList();

            return (newNespapers, newsPaperPages, products);
        }
        catch (Exception ex)
        {
            // Handle any other exceptions here.
            Console.WriteLine("Error in GetNewspapaersInternal: " + ex.Message);
            return (new List<Newspaper>(), new List<NewspaperPage>(), new List<ExtendedProduct>()); // Return empty lists or handle the error as needed.
        }
    }

    public async Task CleanNewspapersAddPages()
    {
        var spots = Repository.GetAllSpots();
        var newspapers = Repository.GetNewspapers();
        var newNespapers = newspapers.Where(x => x.imageUrl.Contains("thumbnailFixedWidth")).Select(x => x.ChangeImageUrl(ParserHelper.ModifyImageUrl(x.imageUrl))).ToList();
        var pageTasks = newNespapers.Select(async paper =>
        {
            try
            {
                var newspaperPages = new List<NewspaperPage>();
                var products = new List<ExtendedProduct>();
                var html = await HtmlSourceManager.DownloadHtmlSourceCode(BaseUrl + paper.url + "#page=1");
                var pageCount = Parser.GetNewspaperPagesCount(html);
                var spot = spots.First(x => x.id == paper.spotId);
                for (int i = 1; i <= pageCount; i++)
                {
                    var newPage = new NewspaperPage(Guid.NewGuid().ToString(), i.ToString(), paper.id, BaseUrl + paper.url + $"#page={i}", ParserHelper.ChangeNumberInUrl(paper.imageUrl, i), DateTimeOffset.UtcNow.ToString());
                    newspaperPages.Add(newPage);
                }
                /*                    for (int i = 1; i <= pageCount; i++)
                                    {
                                        var newHtml = await HtmlSourceManager.DownloadHtmlSourceCode(BaseUrl + paper.url + "#page=" + i.ToString());
                                        var (page, pageProducts) = await Parser.GetNewspaperPage(newHtml, i.ToString(), paper.id, BaseUrl + paper.url + "#page=" + i.ToString(), spot);
                                        newspaperPages.Add(page);
                                        products.AddRange(pageProducts);
                                    }*/
                return (pages: newspaperPages, products: products);
            }
            catch (Exception ex)
            {
                // Handle the exception for this specific task.
                Console.WriteLine("Error in task: " + ex.Message);
                return (pages: new List<NewspaperPage>(), products: new List<ExtendedProduct>());
            }
        });


        var newsPaperPagesLists = await Task.WhenAll(pageTasks);
        var newsPaperPages = newsPaperPagesLists.Select(list => list.pages).SelectMany(list => list).ToList();
        Repository.RemoveNewsppaers();
        Repository.InsertNewspapers(newNespapers);
        Repository.InserNewspapersPages(newsPaperPages);
    }

    public (List<NewspaperPage> pages, List<Product> products) GetNewspapersAndProducts(string html, Spot spot, string newspaperId)
    {
        List<NewspaperPage> pages = new List<NewspaperPage>();
        List<Product> products = new List<Product>();

        try
        {
            string patternPages = "let flyerPages = (.*?);";
            string patternPagesIds = "let flyerPageIds = (.*?);";
            Match matchPages = Regex.Match(html, patternPages);
            Match matchPagesIds = Regex.Match(html, patternPagesIds);

            List<string> pagesIds = new List<string>();
            if (matchPagesIds.Success)
            {
                List<string> newIds = JsonConvert.DeserializeObject<List<string>>(matchPagesIds.Groups[1].Value!)!;
                pagesIds = newIds;
            }
            if (matchPages.Success)
            {
                var value = matchPages.Groups[1].Value;
                List<Page> flyerPages = JsonConvert.DeserializeObject<List<Page>>(value);

                var localPages = flyerPages.Select(x => "https://img.offers-cdn.net" + x.page.Replace("%s", "large"))
                    .Select((x, index) => new NewspaperPage(Guid.NewGuid().ToString(), (index + 1).ToString(), newspaperId, x, x, DateTimeOffset.UtcNow.ToString(), newspaperCode: pagesIds.Count > index ? pagesIds[index] : null));
                pages.AddRange(localPages);
            }

            // Add mappings for products
            string patternProducts = "let hotspots = (.*?);";
            Match matchProducts = Regex.Match(html, patternProducts);
            if (matchProducts.Success)
            {
                var productsValue = matchProducts.Groups[1].Value;
                var productObject = JsonConvert.DeserializeObject<JObject>(productsValue);
                var index = -1;

                foreach (var productEntry in productObject)
                {
                    index++;
                    if (productEntry.Value.Type == JTokenType.Array)
                    {
                        try
                        {
                            var page = pages.FirstOrDefault( x => x.newspaperCode == productEntry.Key );
                            // Handle the array case as before
                            var productsArray = productEntry.Value.ToObject<List<Item>>();
                            foreach (var product in productsArray)
                            {
                                try
                                {

                                    // Create and add Product objects here
                                    Product productRecord = new Product(
                                        id: Guid.NewGuid().ToString(),
                                        name: product.name,
                                        currentPrice: !string.IsNullOrEmpty(product.offer_price)
                                            ? decimal.TryParse(product.offer_price, CultureInfo.InvariantCulture, out var value1) ? value1 : 0
                                            : decimal.TryParse(product.normal_price, CultureInfo.InvariantCulture, out var value2) ? value2 : 0,
                                        oldPrice: !string.IsNullOrEmpty(product.offer_price)
                                            ? decimal.TryParse(product.normal_price, CultureInfo.InvariantCulture, out var value3) ? value3 : 0
                                            : (decimal?)null,
                                        brand: product.store,
                                        priceLabel: product.label,
                                        saleSpecification: product.description,
                                        imageUrl: "https://img.offers-cdn.net" + (product.image.Contains("%s") ? product.image.Replace("%s", "large") : product.image),
                                        newspaperPageId: page?.id,
                                        productCode: product.id.ToString()
                                    ); ;
                                    products.Add(productRecord);
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        try
                        {
                            var productObject2 = productEntry.Value.ToObject<Dictionary<string, Item>>();
                            var page = pages.FirstOrDefault(x => x.newspaperCode == productEntry.Key);

                            foreach (var productKey in productObject2?.Keys)
                            {
                                if (productObject2.TryGetValue(productKey, out var item))
                                {
                                    // Create and add Product objects here
                                    Product productRecord = new Product(
                                        id: Guid.NewGuid().ToString(),
                                        name: item.name,
                                        currentPrice: !string.IsNullOrEmpty(item.offer_price)
                                            ? decimal.TryParse(item.offer_price, NumberStyles.Any, CultureInfo.InvariantCulture, out var value4) ? value4 : 0
                                            : decimal.TryParse(item.normal_price, NumberStyles.Any, CultureInfo.InvariantCulture, out var value5) ? value5 : 0,
                                        oldPrice: !string.IsNullOrEmpty(item.offer_price)
                                            ? decimal.TryParse(item.normal_price, NumberStyles.Any, CultureInfo.InvariantCulture, out var value6) ? value6 : 0
                                            : (decimal?)null,
                                        brand: item.store,
                                        priceLabel: item.label,
                                        saleSpecification: item.description,
                                        imageUrl: item.image != null
                                            ? "https://img.offers-cdn.net" + (item.image.Contains("%s") ? item.image.Replace("%s", "large") : item.image)
                                            : "",
                                        newspaperPageId: page?.id,
                                        productCode: item.id.ToString()

                                    );
                                    products.Add(productRecord);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
        } 
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        

        return (pages, products);
    }

    public async Task<List<Spot>> GetSpotsInternalAsync()
    {
        try
        {
            var htmlTasks = ShopsQualifiers.Select(async x => await HtmlSourceManager.DownloadHtmlWithPuppeteerSharp(ShopsUrl + x));
            var htmlContents = await Task.WhenAll(htmlTasks);

            var spotsList = htmlContents.Select(Parser.GetSpots).SelectMany(s => s).ToList();
            return spotsList;
        }
        catch (Exception ex)
        {
            // Handle the exception here.
            Console.WriteLine("Error in GetSpotsInternalAsync: " + ex.Message);
            return new List<Spot>(); // Return an empty list or handle the error as needed.
        }
    }


    public async Task<(List<ExtendedProduct> products, List<Spot> spots)> GetProductsInternal(string csfr)
    {
        var spots = new List<Spot>();
        var products = new List<ExtendedProduct>();
        try
        {
            spots.AddRange(await GetSpotsInternalAsync());

            var tasks = spots.Select(async x =>
            {
                try
                {
                    var link = BaseUrl + x.url!;
                    var html = await HtmlSourceManager.DownloadHtmlWithPuppeteerSharp(link);
                    var total = Parser.GetProductCount(html);
                    var newUrl = link.Substring(0, link.Length - 7).Replace("sklepy", "stores");
                    var newProducts = await Parser.GetProducts(newUrl, total, csfr);
                    var extenderProducts = newProducts.Select(p => new ExtendedProduct(p, x));
                    return extenderProducts.ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in task: " + ex.Message);
                    return new List<ExtendedProduct>();
                }
            });

            var productLists = await Task.WhenAll(tasks);
            var newProducts = productLists.SelectMany(list => list).ToList();
            products.AddRange(newProducts);
            return (products, spots);
        }
        catch (Exception ex)
        {
            // Handle any other exceptions here.
            Console.WriteLine("Error in GetProductsInternal: " + ex.Message);
            return (products, spots); // Return empty lists or handle the error as needed.
        }
    }
}

public class Page
{
    public string page;
}

public class Item
{
    public double x { get; set; }
    public double y { get; set; }
    public double width { get; set; }
    public double height { get; set; }
    public string link { get; set; }
    public int added { get; set; }
    public int added_mobile { get; set; }
    public string name { get; set; }
    public string store { get; set; }
    public string store_slug { get; set; }
    public int store_id { get; set; }
    public string offer_price { get; set; }
    public string normal_price { get; set; }
    public string currency_code { get; set; }
    public ValidFrom valid_from { get; set; }
    public string description { get; set; }
    public string label { get; set; }
    public int id { get; set; }
    public string slug { get; set; }
    public string start_date { get; set; }
    public string end_date { get; set; }
    public string state { get; set; }
    public string offer_url { get; set; }
    public bool open_direct_link { get; set; }
    public string image { get; set; }
}

public class ValidFrom
{
    public string date { get; set; }
    public int timezone_type { get; set; }
    public string timezone { get; set; }
}