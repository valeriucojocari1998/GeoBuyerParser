using GeoBuyerParser.Managers;
using GeoBuyerParser.Models;
using GeoBuyerParser.Parsers;
using GeoBuyerParser.Repositories;
using HtmlAgilityPack;

namespace GeoBuyerParser.Services;

public record GazetkiService
{
    public Repository Repository { get; }
    public GazetkiParser Parser { get; }

    public ProductService ProductService { get; }

    public static string csrfToken { get; set; } = "";
    public static string BaseUrl { get; } = "https://www.gazetki.pl";
    public static string ShopsUrl { get; } = "https://www.gazetki.pl/sklepy/";
    public List<string> ShopsQualifiers { get; } = new List<string>() { "0-9", "a", "a/2", "b", "b/2", "c", "c/2", "d", "d/2", "e", "f", "g", "h", "i", "j", "k", "k/2", "l", "m", "m/2", "n", "o", "p", "p/2", "q", "r", "s", "s/2", "s/3", "t", "t/2", "u", "v", "w", "x", "y", "z" };

    public GazetkiService(Repository repository, GazetkiParser parser, ProductService productService)
    {
        Repository = repository;
        Parser = parser;
        ProductService = productService;
    }

    public static async Task GetCSRF()
    {
        var html = await HtmlSourceManager.DownloadHtmlSourceCode(BaseUrl);
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        var scriptNode = doc.DocumentNode.SelectSingleNode("//script[contains(., 'window.csrf')]");
        if (scriptNode == null)
            throw new Exception("CSRF not parrsed");

        string scriptContent = scriptNode.InnerHtml;
        string pattern = @"window\.csrf\s*=\s*""([^""]+)"";";
        var match = System.Text.RegularExpressions.Regex.Match(scriptContent, pattern);
        csrfToken = match.Groups[1].Value;
    }

    public async Task<List<ExtendedProduct>> GetProducts()
    {
        var products = new List<ExtendedProduct>();
        var data = await GetProductsInternal();
        products.AddRange(data.products);

        var newsPapers = await GetNewspapaersInternal(data.spots);
        ProductService.AddOrOverrideProducts(newsPapers.products);
        Repository.InsertNewspapers(newsPapers.newspapers);
        Repository.InserNewspapersPages(newsPapers.pages);

        return products;
    }

    public async Task<(List<Newspaper> newspapers, List<NewspaperPage> pages, List<ExtendedProduct> products)> GetNewspapaersInternal(List<Spot> spots)
    {
        var tasks = spots.Select(async spot =>
        {
            var url = BaseUrl + spot.url;
            var html = await HtmlSourceManager.DownloadHtmlSourceCode(url);
            var spotNews = Parser.GetNewspapers(html, spot.id);
            return spotNews;
        });
        var newsPaperLists = await Task.WhenAll(tasks);
        var newspapers = newsPaperLists.SelectMany(list => list).ToList();

        var pageTasks = newspapers.Select(async paper =>
        {
            var newspaperPages = new List<NewspaperPage>();
            var products = new List<ExtendedProduct>();
            var html = await HtmlSourceManager.DownloadHtmlSourceCode(BaseUrl + paper.url + "#page=1");
            var pageCount = Parser.GetNewspaperPagesCount(html);
            var spot = spots.First(x => x.id == paper.spotId);
            for (int i = 1; i <= pageCount; i++)
            {
                var newHtml = await HtmlSourceManager.DownloadHtmlSourceCode(BaseUrl + paper.url + "#page=" + i.ToString());
                var (page, pageProducts) = await Parser.GetNewspaperPage(newHtml, i.ToString(), paper.id, BaseUrl + paper.url + "#page=" + i.ToString(), spot);
                newspaperPages.Add(page);
                products.AddRange(pageProducts);
            }
            return (pages: newspaperPages, products);
        });

        var newsPaperPagesLists = await Task.WhenAll(pageTasks);
        var newsPaperPages = newsPaperPagesLists.Select(list => list.pages).SelectMany(list => list).ToList();
        var products = newsPaperPagesLists.Select(list => list.products).SelectMany(list => list).ToList();



        return (newspapers, newsPaperPages, products);
    }

    public async Task<List<Spot>> GetSpotsInternal()
    {
        var htmls = await Task.WhenAll(ShopsQualifiers.Select(x => ShopsUrl + x).ToList().Select(async x => await HtmlSourceManager.DownloadHtmlSourceCode(x)));
        return htmls.Select(x => Parser.GetSpots(x)).SelectMany(s => s).ToList();
    }
    public async Task<(List<ExtendedProduct> products, List<Spot> spots)> GetProductsInternal()
    {
        var spots = await GetSpotsInternal();

        var tasks = spots.Select(async x =>
        {
            var link = BaseUrl + x.url!;
            var html = await HtmlSourceManager.DownloadHtmlSourceCode(link);
            var total = Parser.GetProductCount(html);
            var newUrl = link.Substring(0, link.Length - 7).Replace("sklepy", "stores");
            var newProducts = await Parser.GetProducts(newUrl, total, csrfToken);
            var extenderProducts = newProducts.Select(p => new ExtendedProduct(p, x));
            return extenderProducts.ToList();
        });

        var productLists = await Task.WhenAll(tasks);
        var products = productLists.SelectMany(list => list).ToList();
        Repository.InsertSpots(spots);
        ProductService.AddOrOverrideProducts(products);
        return (products, spots);
    }
}
