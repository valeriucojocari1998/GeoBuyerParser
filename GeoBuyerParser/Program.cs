using GeoBuyerParser.DB;
using GeoBuyerParser.Parsers;
using GeoBuyerParser.Repositories;
using GeoBuyerParser.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        var serviceProvider = host.Services;

        var biedronkaService = serviceProvider.GetRequiredService<BiedronkaService>();
        var kauflandService = serviceProvider.GetRequiredService<KauflandService>();
        var lidlService = serviceProvider.GetRequiredService<LidlService>();
        var sparService = serviceProvider.GetRequiredService<SparService>();

        await biedronkaService.GetProducts();
        await kauflandService.GetProducts();
        await lidlService.GetProducts();
        await sparService.GetProducts();

        // Stop the application gracefully
        await host.StopAsync();
    }


    public static IHostBuilder CreateHostBuilder(string[] args) =>
     Host.CreateDefaultBuilder(args)
         .ConfigureServices((hostContext, services) =>
         {
             // Register DbContext as Scoped
             services.AddDbContext<AppDbContext>(options =>
             {
                 options.UseSqlite("Data Source=C:/Users/Alvys-Dev/Documents/Valeriu-Projects/GeoBuyerParser/GeoBuyerParser/app.db");
             }, ServiceLifetime.Singleton);

             // Register Parsers as Singleton
             services.AddSingleton<BiedronkaParser>();
             services.AddSingleton<KauflandParser>();
             services.AddSingleton<LidlParser>();
             services.AddSingleton<SparParser>();

             // Register Repository as Scoped
             services.AddSingleton<Repository>();

             // Register Services as Scoped
             services.AddScoped<BiedronkaService>();
             services.AddScoped<KauflandService>();
             services.AddScoped<LidlService>();
             services.AddScoped<SparService>();
         });
}
