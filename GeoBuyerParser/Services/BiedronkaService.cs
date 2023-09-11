using GeoBuyerParser.Enums;
using GeoBuyerParser.Managers;
using GeoBuyerParser.Models;
using GeoBuyerParser.Parsers;
using GeoBuyerParser.Repositories;

namespace GeoBuyerParser.Services;

public record BiedronkaService
{
    public Repository Repository { get; }
    public BiedronkaParser Parser { get; }
    public string Spot { get; } = SpotProvider.Biedronka;
    public string SpotUrl { get; } = "https://home.biedronka.pl/promocje";
    public string SpotUrlAddition { get; } = "?start=0&sz=1000";

    public BiedronkaService(Repository repository, BiedronkaParser parser)
    {
        Repository = repository;
        Parser = parser;
    }
    public async Task<int> GetProducts()
    {
        var spot = Repository.GetSpotByProvider(Spot);
        var categories = await GetCategories();
        var newCategories = new List<Category>();
        var productsLists = await Task.WhenAll(categories.Select(async category =>
        {
            var (newCategory, products) = await GetProductsByCategory(category);
            newCategories.Add(newCategory);
            return products.Select(product => new ExtendedProduct(product, spot, category));
        }));

        var extendedCategories = newCategories.Select(cat => new ExtendedCategory(cat, spot)).ToList();
        var extendedProducts = productsLists.SelectMany(pr => pr).ToList();
        Repository.InsertCategories(extendedCategories);
        Repository.InsertProducts(extendedProducts);
        return extendedProducts?.Count ?? 0;
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
