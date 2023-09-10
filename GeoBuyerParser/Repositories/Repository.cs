using GeoBuyerParser.DB;
using GeoBuyerParser.Models;

namespace GeoBuyerParser.Repositories;


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
public class Repository
{
    private readonly AppDbContext _dbContext;

    public Repository(AppDbContext dbContext)
    {
        _dbContext = dbContext;

    }

    public Spot GetSpotById(string id)
    {
        try
        {
            return _dbContext.Spots.First(x => x.id == id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public Spot GetSpotByProvider(string spotName)
    {
        try
        {
            return _dbContext.Spots.First(x => x.provider == spotName);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public IEnumerable<Spot> GetAllSpots()
    {
        return _dbContext.Spots.ToList();
    }

    public void InsertSpots(IEnumerable<Spot> spots)
    {
        _dbContext.Spots.AddRange(spots);
        _dbContext.SaveChanges();
    }

    public List<ExtendedCategory> GetCategories()
    {
        return _dbContext.Categories.ToList();

    }
    public void InsertCategories(IEnumerable<ExtendedCategory> categories)
    {
        _dbContext.Categories.AddRange(categories);
        _dbContext.SaveChanges();
    }

    public List<ExtendedProduct> GetProducts()
    {
        return _dbContext.Products.ToList();
    }

    public void InsertProducts(IEnumerable<ExtendedProduct> products)
    {
        _dbContext.Products.AddRange(products);
        _dbContext.SaveChanges();
    }

    public void DeleteProductsByIds(List<string> productIds)
    {
        try
        {
            // Find the products to delete based on their IDs
            var productsToDelete = _dbContext.Products.Where(p => productIds.Contains(p.id)).ToList();

            if (productsToDelete.Any())
            {
                _dbContext.Products.RemoveRange(productsToDelete);
                _dbContext.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public void InsertNewspapers(IEnumerable<Newspaper> newspapers)
    {
        _dbContext.Newspapers.AddRange(newspapers);
        _dbContext.SaveChanges();
    }

    public void InserNewspapersPages(IEnumerable<NewspaperPage> pages)
    {
        _dbContext.NewspaperPages.AddRange(pages);
        _dbContext.SaveChanges();
    }

}
