using GeoBuyerParser.Enums;
using GeoBuyerParser.Managers;
using GeoBuyerParser.Models;
using GeoBuyerParser.Parsers;
using GeoBuyerParser.Repositories;

namespace GeoBuyerParser.Services;

public record GazetkiService
{
    public Repository Repository { get; }
    public GazetkiParser Parser { get; }
    public string BaseUrl { get; } = "https://www.gazetki.pl";
    public string ShopsUrl { get; } = "https://www.gazetki.pl/sklepy/";
    public List<string> ShopsQualifiers { get; } = new List<string>() { "0-9", "a", "a/2", "b", "b/2", "c", "c/2", "d", "d/2", "e", "f", "g", "h", "i", "j", "k", "k/2", "l", "m", "m/2", "n", "o", "p", "p/2", "q", "r", "s", "s/2", "s/3", "t", "t/2", "u", "v", "w", "x", "y", "z" };

    public GazetkiService(Repository repository, GazetkiParser parser)
    {
        Repository = repository;
        Parser = parser;
    }
    public async Task GetProducts()
    {
        var spots = await GetSpotsInternal();

        var tasks = spots.Select(async x =>
        {
            var link = BaseUrl + x.url!;
            var html = await HtmlSourceManager.DownloadHtmlSourceCode(link);
            var total = Parser.GetProductCount(html);
            var newUrl = link.Substring(0, link.Length - 7);
            var newProducts = await Parser.GetProducts(newUrl, total);
            var extenderProducts = newProducts.Select(p => new ExtendedProduct(p, x));
            return extenderProducts.ToList();
        });

        var productLists = await Task.WhenAll(tasks);
        var products = productLists.SelectMany(list => list).ToList();
        Repository.InsertSpots(spots);
        Repository.InsertProducts(products);

    }

    public async Task<List<Spot>> GetSpotsInternal()
    {
        var htmls = await Task.WhenAll(ShopsQualifiers.Select(x => ShopsUrl + x).ToList().Select(async x => await HtmlSourceManager.DownloadHtmlSourceCode(x)));
        return htmls.Select(x => Parser.GetSpots(x)).SelectMany(s => s).ToList();
    }
}
