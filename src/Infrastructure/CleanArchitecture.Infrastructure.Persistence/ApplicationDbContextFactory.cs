using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Infrastructure.Persistence;

/// <summary>
/// Factory for instantiating ApplicationDbContext at design-time (e.g., dotnet ef migrations).
/// This prevents dependency injection and SSPI errors on Linux/macOS environments where WebAPI Host 
/// fails to start due to Trusted_Connection SQL Server issues.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = Directory.GetCurrentDirectory();

        // Depending on where 'dotnet ef' is invoked from, the base path could be the solution root, 
        // the Persistence project, or the WebAPI startup project.
        // We will try to locate the WebAPI appsettings.json robustly.
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            var fallbackPath = Path.Combine(basePath, "../../Presentation/CleanArchitecture.WebAPI");
            if (File.Exists(Path.Combine(fallbackPath, "appsettings.json")))
            {
                basePath = fallbackPath;
            }
            else
            {
                // Another fallback if running from exactly solution root
                var rootFallbackPath = Path.Combine(basePath, "src/Presentation/CleanArchitecture.WebAPI");
                if (File.Exists(Path.Combine(rootFallbackPath, "appsettings.json")))
                {
                    basePath = rootFallbackPath;
                }
            }
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{envName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.");

        builder.UseSqlServer(connectionString);

        return new ApplicationDbContext(builder.Options);
    }
}
