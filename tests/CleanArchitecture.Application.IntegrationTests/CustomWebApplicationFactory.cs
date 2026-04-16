using CleanArchitecture.Infrastructure.Identity;
using CleanArchitecture.Infrastructure.Identity.Models;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Application.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory yang mengganti SQL Server dengan SQLite in-memory
/// dan meng-skip Hangfire agar tidak membutuhkan koneksi database eksternal saat testing.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Unique DB name per factory instance to avoid test interference
    private readonly string _applicationDbName = $"TestAppDb_{Guid.NewGuid()}";
    private readonly string _identityDbName = $"TestIdentityDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // ── Replace ApplicationDbContext (SQL Server → SQLite in-memory) ──────────
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite($"Data Source={_applicationDbName};Mode=Memory;Cache=Shared"));

            // ── Replace IdentityDbContext (SQL Server → SQLite in-memory) ─────────────
            services.RemoveAll<DbContextOptions<IdentityDbContext>>();
            services.RemoveAll<IdentityDbContext>();
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlite($"Data Source={_identityDbName};Mode=Memory;Cache=Shared"));

            // ── Skip Hangfire (requires real SQL Server) ──────────────────────────────
            // Hangfire is registered in Program.cs before builder.Build(), so we patch
            // the connection string check by overriding the Hangfire storage to in-memory.
            // The simplest approach: remove Hangfire-related hosted services.
            var hangfireDescriptors = services
                .Where(d => d.ServiceType.Namespace != null &&
                            d.ServiceType.Namespace.StartsWith("Hangfire"))
                .ToList();
            foreach (var d in hangfireDescriptors)
                services.Remove(d);
        });

        builder.ConfigureServices(async services =>
        {
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedSp = scope.ServiceProvider;

            // Ensure databases are created
            var appDb = scopedSp.GetRequiredService<ApplicationDbContext>();
            await appDb.Database.EnsureCreatedAsync();

            var identityDb = scopedSp.GetRequiredService<IdentityDbContext>();
            await identityDb.Database.EnsureCreatedAsync();

            // Seed roles and test users
            var roleManager = scopedSp.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scopedSp.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { "Admin", "Member" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Test admin user
            if (await userManager.FindByEmailAsync("testadmin@test.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "testadmin@test.com",
                    Email = "testadmin@test.com",
                    FullName = "Test Admin"
                };
                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Test member user
            if (await userManager.FindByEmailAsync("testmember@test.com") == null)
            {
                var member = new ApplicationUser
                {
                    UserName = "testmember@test.com",
                    Email = "testmember@test.com",
                    FullName = "Test Member"
                };
                var result = await userManager.CreateAsync(member, "Member123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(member, "Member");
            }
        });
    }
}
