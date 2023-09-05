using GeoBuyerParser.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoBuyerParser.DB;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
    {
        Database.EnsureCreated();
    }

    public DbSet<Spot> Spots { get; set; }
    public DbSet<ExtendedCategory> Categories { get; set; }
    public DbSet<ExtendedProduct> Products { get; set; }
}