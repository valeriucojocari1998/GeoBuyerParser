using GeoBuyerParser.Enums;
using GeoBuyerParser.Managers;
using GeoBuyerParser.Models;
using GeoBuyerParser.Parsers;
using GeoBuyerParser.Repositories;

namespace GeoBuyerParser.Services;

public record KauflandService
{
    public Repository Repository { get; }
    public KauflandParser Parser { get; }
    public string Spot { get; } = SpotProvider.Kaufland;
    public string SpotUrl { get; } = "https://www.kaufland.pl/oferta/aktualny-tydzien/przeglad.category=01_Mi%C4%99so__Dr%C3%B3b__W%C4%99dliny.html";
    public string SpotUrlAddition { get; } = "";

    public KauflandService(Repository repository, KauflandParser parser)
    {
        Repository = repository;
        Parser = parser;
    }
    public async Task<List<ExtendedProduct>> GetProducts()
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
        return extendedProducts;
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
