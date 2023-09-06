using GeoBuyerParser.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        // Start the background service
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var repository = scope.ServiceProvider.GetRequiredService<Repository>();
            var spots = repository.GetAllSpots();
            var configSpots = RepositoryConfig.Spots;
            var missing = configSpots.Except(spots).ToList();
            repository.InsertSpots(missing);
            await host.RunAsync();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://*:8080");
            });
}
