using GeoBuyerPromotion.Managers;
using GeoBuyerPromotion.Models;
using GeoBuyerPromotion.Parsers;
using GeoBuyerPromotion.Repositories;

namespace GeoBuyerPromotion.Services;

public record SpotService: ISpotService
{
    public CsvManager CsvManager { get; }
    public IRepository Repository { get; }
    public IParser Parser { get; }
    public string Spot { get; }
    public string SpotUrl { get; }
    public string SpotUrlAddition { get; }

    public SpotService(CsvManager csvManager, IRepository repository, IParser parser, string market, string marketUrl, string spotUrlAddition)
    {
        CsvManager = csvManager;
        Repository = repository;
        Parser = parser;
        Spot = market;
        SpotUrl = marketUrl;
        SpotUrlAddition = spotUrlAddition;
    }
    public async Task GetProducts()
    {
        var spot = Repository.GetSpotByProvider(Spot);
        var categories = await GetCategories();
        var newCategories = new List<Category>();
        var productsLists = await Task.WhenAll(categories.Select(async category =>
        {
            var (newCategory, products) = await GetProductsByCategory(category);
            newCategories.Add(newCategory);
            return products.Select(product => new ExtendedProduct(product, category, spot));
        }));

        var extendedCategories = newCategories.Select(cat => new ExtendedCategory(cat, spot)).ToList();
        var extendedProducts = productsLists.SelectMany(pr => pr).ToList();
        CsvManager.WriteListsToCsv(spot, extendedCategories, extendedProducts);
    }

    public async Task<List<Category>> GetCategories()
    {
        var data = await HtmlSourceManager.DownloadHtmlSourceCode(SpotUrl);
        return Parser.GetCategories(data);
    }

    public async Task<(Category newCategory, List<Product> products)> GetProductsByCategory(Category category)
    {
        var url = category.categoryUrl + SpotUrlAddition;
        var data = await HtmlSourceManager.DownloadHtmlSourceCode(url);
        var products = Parser.GetProductsByCategory(data, category.name);
        var newCategory = category.AddCount(products.Count);
        return (newCategory, products);
    }
}
