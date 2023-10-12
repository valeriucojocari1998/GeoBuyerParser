using GeoBuyerParser.Helpers;
using GeoBuyerParser.Managers;
using GeoBuyerParser.Models;
using GeoBuyerParser.Parsers;
using GeoBuyerParser.Repositories;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Globalization;
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

        var html = await HtmlSourceManager.DownloadHtmlSourceCode(BaseUrl);
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        var scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(., 'window.csrf')]");
        if (scriptNode == null)
            throw new Exception("CSRF not parsed");

        string scriptContent = scriptNode.InnerHtml;
        string pattern = @"window\.csrf\s*=\s*""([^""]+)"";";
        var match = System.Text.RegularExpressions.Regex.Match(scriptContent, pattern);
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
                    var newPage = new NewspaperPage(Guid.NewGuid().ToString(), i.ToString(), paper.id, BaseUrl + paper.url + $"#page={i}", ParserHelper.ChangeNumberInUrl(paper.imageUrl, i));
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

    public (List<NewspaperPage> pages, List<Product> products) GetNewspapersAndProducts(string html, string newspaperId)
    {
        List<NewspaperPage> pages = new List<NewspaperPage>();
        List<Product> products = new List<Product>();

        string patternPages = "let flyerPages = (.*?);";
        Match matchPages = Regex.Match(html, patternPages);
        if (matchPages.Success)
        {
            var value = matchPages.Groups[1].Value;
            List<string> flyerPages = JsonConvert.DeserializeObject<List<string>>(value);
            var localPages = flyerPages.Select(x => "https://img.offers-cdn.net" + x.Replace("%s", "large"))
                .Select((x, index) => new NewspaperPage(Guid.NewGuid().ToString(), index.ToString(), newspaperId, x, x));
            pages.AddRange(localPages);
        }

        // Add mappings for products
        string patternProducts = "let hotspots = (.*?);";
        Match matchProducts = Regex.Match(html, patternProducts);
        if (matchProducts.Success)
        {
            var productsValue = matchProducts.Groups[1].Value;
            Dictionary<string, Dictionary<string, Item>> productDictionary =
                JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Item>>>(productsValue);

            foreach (var entry in productDictionary)
            {
                foreach (var item in entry.Value)
                {
                    Product product = new Product()
                    
                        // Map properties from item to product
                        Name = item.Value.name,
                        CurrentPrice = !string.IsNullOrEmpty(item.Value.offer_price)
                            ? decimal.Parse(item.Value.offer_price, CultureInfo.InvariantCulture)
                            : decimal.Parse(item.Value.normal_price, CultureInfo.InvariantCulture),
                        OldPrice = !string.IsNullOrEmpty(item.Value.offer_price)
                            ? decimal.Parse(item.Value.normal_price, CultureInfo.InvariantCulture)
                            : (decimal?)null,
                        Brand = item.Value.store,
                        PriceLabel = item.Value.label,
                        SaleSpecification = item.Value.description,
                        ImageUrl = "https://img.offers-cdn.net" + item.Value.image.Replace("%s", "large"),
                        // Add other mappings as needed
                    )

                    products.Add(product);
                }
            }
        }

        return (pages, products);
    }

    public async Task<List<Spot>> GetSpotsInternal()
    {
        try
        {
            var htmls = await Task.WhenAll(ShopsQualifiers.Select(x => ShopsUrl + x).ToList().Select(async x => await HtmlSourceManager.DownloadHtmlSourceCode(x)));
            return htmls.Select(x => Parser.GetSpots(x)).SelectMany(s => s).ToList();
        }
        catch (Exception ex)
        {
            // Handle the exception here.
            Console.WriteLine("Error in GetSpotsInternal: " + ex.Message);
            return new List<Spot>(); // Return an empty list or handle the error as needed.
        }
    }

    public async Task<(List<ExtendedProduct> products, List<Spot> spots)> GetProductsInternal(string csfr)
    {
        var spots = new List<Spot>();
        var products = new List<ExtendedProduct>();
        try
        {
            spots.AddRange(await GetSpotsInternal());

            var tasks = spots.Select(async x =>
            {
                try
                {
                    var link = BaseUrl + x.url!;
                    var html = await HtmlSourceManager.DownloadHtmlSourceCode(link);
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