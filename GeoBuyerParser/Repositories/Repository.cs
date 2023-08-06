using GeoBuyerPromotion.Models;

namespace GeoBuyerPromotion.Repositories;


// This Config will be removed when the db will be ready
public record RepositoryConfig
{
    public static List<Spot> Spots = new List<Spot>()
    {
        new Spot("a29a43c6-b9ec-4873-b2c4-2861f09dc1c9", "Biedronka"),
        new Spot("d78b8214-19ec-4a98-97c4-0b9cfbf341a0", "Kaufland"),
        new Spot("21962942-db00-4bd2-b5fc-3dda13a61b49", "Lidl"),
        new Spot("bd3c661b-0984-453d-9473-dc738c79c0bf", "Spar")
    };
}
public record Repository : IRepository
{
    public string Container { get; }
    public string DbName { get; }



    public Repository(string container, string dbName)
    {
        this.Container = container;
        this.DbName = dbName;
    }

    public Spot GetSpotByProvider(string spotProvider)
    {
        return RepositoryConfig.Spots.FirstOrDefault(x => x.provider == spotProvider)!;
    }

    public List<Spot> GetSpots()
    {
        return RepositoryConfig.Spots;
    }
    public void InsertSpots(List<Spot> markets)
    {
        throw new NotImplementedException();
    }

    public List<ExtendedCategory> GetCategories()
    {
        throw new NotImplementedException();
    }

    public void InsertCategories(List<ExtendedCategory> category)
    {
        throw new NotImplementedException();
    }

    public List<ExtendedProduct> GetProducts()
    {
        throw new NotImplementedException();
    }

    public void InsertProducts(List<ExtendedProduct> product)
    {
        throw new NotImplementedException();
    }
}
