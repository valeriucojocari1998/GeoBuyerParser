using GeoBuyerPromotion.Models;

namespace GeoBuyerPromotion.Repositories;

public interface IRepository
{
    public string Container { get; }
    public string DbName { get; }

    /// <summary>
    ///  Spot methods
    /// </summary>
    public Spot GetSpotByProvider(string spotName);
    public List<Spot> GetSpots();
    public void InsertSpots(List<Spot> markets);

    /// <summary>
    /// Category methods
    /// </summary>
    public List<ExtendedCategory> GetCategories();
    public void InsertCategories(List<ExtendedCategory> category);

    /// <summary>
    /// Product methods
    /// </summary>
    public List<ExtendedProduct> GetProducts();
    public void InsertProducts(List<ExtendedProduct> product);
    
}
