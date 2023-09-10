

using GeoBuyerParser.Models;
using GeoBuyerParser.Repositories;

namespace GeoBuyerParser.Services;

public class ProductService
{
    public Repository Repository { get; }

    public ProductService(Repository repository)
    {
        Repository = repository;
    }

    public List<ExtendedProduct> GetProducts()
    {
        return Repository.GetProducts();
    }

    public void AddOrOverrideProducts(List<ExtendedProduct> products)
    {
        var ids = products.Select(x => x.id).ToList();
        Repository.DeleteProductsByIds(ids);
        Repository.InsertProducts(products);
    }
}
