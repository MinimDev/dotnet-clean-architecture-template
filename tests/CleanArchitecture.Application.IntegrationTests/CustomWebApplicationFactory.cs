using CleanArchitecture.Infrastructure.Identity;
using CleanArchitecture.Infrastructure.Identity.Models;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Application.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory yang mengganti SQL Server dengan SQLite in-memory
/// dan meng-skip Hangfire agar tidak membutuhkan koneksi database eksternal saat testing.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _applicationDbName = $"TestAppDb_{Guid.NewGuid()}";
    private readonly string _identityDbName = $"TestIdentityDb_{Guid.NewGuid()}";
    private SqliteConnection? _appConnection;
    private SqliteConnection? _identityConnection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // ── Remove previous DbContext and SqlServer registrations ──
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions<IdentityDbContext>) ||
                d.ServiceType == typeof(ApplicationDbContext) ||
                d.ServiceType == typeof(IdentityDbContext) ||
                d.ServiceType.FullName?.Contains("DbContextOptions") == true ||
                d.ServiceType.FullName?.Contains("SqlServer") == true ||
                d.ImplementationType?.FullName?.Contains("SqlServer") == true ||
                d.ServiceType.Assembly.GetName().Name?.Contains("SqlServer") == true ||
                d.ImplementationType?.Assembly.GetName().Name?.Contains("SqlServer") == true
            ).ToList();

            foreach (var d in descriptorsToRemove)
                services.Remove(d);

            _appConnection = new SqliteConnection($"Data Source={_applicationDbName};Mode=Memory;Cache=Shared");
            _appConnection.Open();

            _identityConnection = new SqliteConnection($"Data Source={_identityDbName};Mode=Memory;Cache=Shared");
            _identityConnection.Open();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<CleanArchitecture.Infrastructure.Persistence.Interceptors.AuditableEntityInterceptor>();
                options.UseSqlite(_appConnection);
                options.AddInterceptors(interceptor);
            });

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlite(_identityConnection));

            // ── Skip Hangfire ───────────────────────────────────────────────────────────
            var hangfireDescriptors = services
                .Where(d => d.ServiceType.Namespace != null &&
                            d.ServiceType.Namespace.StartsWith("Hangfire"))
                .ToList();
            foreach (var d in hangfireDescriptors)
                services.Remove(d);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = builder.Build();
        host.Start();

        using (var scope = host.Services.CreateScope())
        {
            var scopedSp = scope.ServiceProvider;

            // Ensure SQLite databases are created
            var appDb = scopedSp.GetRequiredService<ApplicationDbContext>();
            appDb.Database.EnsureCreated();

            var identityDb = scopedSp.GetRequiredService<IdentityDbContext>();
            identityDb.Database.EnsureCreated();

            // Seed roles and test users
            var roleManager = scopedSp.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scopedSp.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { "Admin", "Member" })
            {
                if (!roleManager.RoleExistsAsync(role).GetAwaiter().GetResult())
                    roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
            }

            if (userManager.FindByEmailAsync("testadmin@test.com").GetAwaiter().GetResult() == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "testadmin@test.com",
                    Email = "testadmin@test.com",
                    FullName = "Test Admin"
                };
                var result = userManager.CreateAsync(admin, "Admin123!").GetAwaiter().GetResult();
                if (result.Succeeded)
                    userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
            }

            if (userManager.FindByEmailAsync("testmember@test.com").GetAwaiter().GetResult() == null)
            {
                var member = new ApplicationUser
                {
                    UserName = "testmember@test.com",
                    Email = "testmember@test.com",
                    FullName = "Test Member"
                };
                var result = userManager.CreateAsync(member, "Member123!").GetAwaiter().GetResult();
                if (result.Succeeded)
                    userManager.AddToRoleAsync(member, "Member").GetAwaiter().GetResult();
            }
        }

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _appConnection?.Dispose();
            _identityConnection?.Dispose();
        }
    }
}
