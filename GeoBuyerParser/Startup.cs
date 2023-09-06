using GeoBuyerParser.DB;
using GeoBuyerParser.Parsers;
using GeoBuyerParser.Repositories;
using GeoBuyerParser.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

public class Startup
{
    private IWebHostEnvironment _env;

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        _env = env;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite($"Data Source={_env.ContentRootPath}/app.db");
        }, ServiceLifetime.Singleton);

        services.AddSingleton<BiedronkaParser>();
        services.AddSingleton<KauflandParser>();
        services.AddSingleton<LidlParser>();
        services.AddSingleton<SparParser>();
        services.AddSingleton<GazetkiParser>();

        services.AddSingleton<Repository>();

        services.AddScoped<BiedronkaService>();
        services.AddScoped<KauflandService>();
        services.AddScoped<LidlService>();
        services.AddScoped<SparService>();
        services.AddScoped<GazetkiService>();

        // Add necessary services for the web API
        services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
