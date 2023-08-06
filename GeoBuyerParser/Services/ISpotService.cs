using GeoBuyerPromotion.Managers;
using GeoBuyerPromotion.Models;
using GeoBuyerPromotion.Parsers;
using GeoBuyerPromotion.Repositories;

namespace GeoBuyerPromotion.Services;

interface ISpotService
{
    CsvManager CsvManager { get; }
    IRepository Repository { get; }
    IParser Parser { get; }
    string Spot { get; }
    string SpotUrl { get; }
    string SpotUrlAddition { get; }
    public Task GetProducts();
    public Task<List<Category>> GetCategories();
    public Task<(Category newCategory, List<Product> products)> GetProductsByCategory(Category category);
}
