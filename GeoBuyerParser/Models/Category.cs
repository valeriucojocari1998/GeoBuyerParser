namespace GeoBuyerParser.Models;

public record Category(string id, string name, string categoryUrl, int? productCount = null)
{
    public Category AddCount(int x) => this with
    {
        productCount = x,
    };
}

public record ExtendedCategory(string id, string name, int productCount, string marketId, string marketProvider, string categoryUrl)
{
    public ExtendedCategory(Category category, Spot market)
        : this(category.id, category.name, category.productCount ?? 0, market.id, market.provider, category.categoryUrl)
    { }
}

