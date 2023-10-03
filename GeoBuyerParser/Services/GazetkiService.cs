using GeoBuyerParser.Helpers;
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
                    var newPage = new NewspaperPage(new Guid().ToString(), i.ToString(), paper.id, BaseUrl + paper.url + $"#page={i}", ParserHelper.ChangeNumberInUrl(paper.imageUrl, i));
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

    public async Task<(List<Newspaper> newspapers, List<NewspaperPage> pages, List<ExtendedProduct> products)> GetNewspapaersInternal(List<Spot> spots)
    {
        try
        {
            var tasks = spots.Select(async spot =>
            {
                try
                {
                    var url = BaseUrl + spot.url;
                    var html = await HtmlSourceManager.DownloadHtmlSourceCode(url);
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
                    var newspaperPages = new List<NewspaperPage>();
                    var products = new List<ExtendedProduct>();
                    var html = await HtmlSourceManager.DownloadHtmlSourceCode(BaseUrl + paper.url + "#page=1");
                    var pageCount = Parser.GetNewspaperPagesCount(html);
                    var spot = spots.First(x => x.id == paper.spotId);
                    for (int i = 1; i <= pageCount;i++)
                    {
                        var newPage = new NewspaperPage(new Guid().ToString(), i.ToString(), paper.id, BaseUrl + paper.url + $"#page={i}", ParserHelper.ChangeNumberInUrl(paper.imageUrl, i));
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
