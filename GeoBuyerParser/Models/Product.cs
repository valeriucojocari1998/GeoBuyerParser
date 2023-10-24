namespace GeoBuyerParser.Models;

public record Product(
    string id,
    string name,
    decimal currentPrice,
    decimal? oldPrice = null,
    string? brand = null,
    string? priceLabel = null,
    string? saleSpecification = null,
    string? imageUrl = null,
    string? newspaperPageId = null,
    string? productCode = null);

public record ExtendedProduct(
    string id,
    string name,
    DateTimeOffset dateCreated,
    decimal currentPrice,
    string marketId,
    string marketProvider,
    string? categoryId = null,
    string? categoryName = null,
    decimal? oldPrice = null,
    string? brand = null,
    string? priceLabel = null,
    string? saleSpecification = null,
    string? imageUrl = null,
    string? newspaperPageId = null,
    string? productCode = null)
{
    public ExtendedProduct(Product product, Spot market, Category? category = null)
        : this(
              id: product.id,
              name: product.name,
              dateCreated: DateTimeOffset.UtcNow,
              currentPrice: product.currentPrice,
              oldPrice: product.oldPrice,
              brand: product.brand,
              priceLabel: product.priceLabel,
              saleSpecification: product.saleSpecification,
              imageUrl: product.imageUrl,
              categoryId: category?.id,
              categoryName: category?.name,
              marketId: market.id,
              marketProvider: market.provider,
              newspaperPageId: product.newspaperPageId,
              productCode: product.productCode)
    { }
}


