using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Infrastructure.Identity;

/// <summary>
/// Factory for instantiating IdentityDbContext at design-time (e.g., dotnet ef migrations).
/// This prevents dependency injection and SSPI errors on Linux/macOS environments where WebAPI Host 
/// fails to start due to Trusted_Connection SQL Server issues.
/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var basePath = Directory.GetCurrentDirectory();

        // Robust path resolution to find WebAPI appsettings.json
        if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            var fallbackPath = Path.Combine(basePath, "../../Presentation/CleanArchitecture.WebAPI");
            if (File.Exists(Path.Combine(fallbackPath, "appsettings.json")))
            {
                basePath = fallbackPath;
            }
            else
            {
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

        var builder = new DbContextOptionsBuilder<IdentityDbContext>();
        var connectionString = configuration.GetConnectionString("IdentityConnection")
                               ?? throw new InvalidOperationException("Connection string 'IdentityConnection' not found in appsettings.");

        builder.UseSqlServer(connectionString);

        return new IdentityDbContext(builder.Options);
    }
}
