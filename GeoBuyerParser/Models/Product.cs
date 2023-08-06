namespace GeoBuyerPromotion.Models;

public record Product(
    string id,
    string name,
    decimal currentPrice,
    decimal? oldPrice = null,
    string? brand = null,
    string? priceLabel = null,
    string? saleSpecification = null,
    string? imageUrl = null);

public record ExtendedProduct(
    string id,
    string name,
    decimal currentPrice,
    string categoryId,
    string categoryName,
    string marketId,
    string marketProvider,
    decimal? oldPrice = null,
    string? brand = null,
    string? priceLabel = null,
    string? saleSpecification = null,
    string? imageUrl = null)
{
    public ExtendedProduct(Product product, Category category, Spot market)
        : this(
              id: product.id,
              name: product.name,
              currentPrice: product.currentPrice,
              oldPrice: product.oldPrice,
              brand: product.brand,
              priceLabel: product.priceLabel,
              saleSpecification: product.saleSpecification,
              imageUrl: product.imageUrl,
              categoryId: category.id,
              categoryName: category.name,
              marketId: market.id,
              marketProvider: market.provider)
    { }
}


