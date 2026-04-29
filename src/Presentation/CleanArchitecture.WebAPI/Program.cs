using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CleanArchitecture.Application;
using CleanArchitecture.Infrastructure.Identity;
using CleanArchitecture.Infrastructure.Identity.Models;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Infrastructure.Shared;
using CleanArchitecture.WebAPI.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using CleanArchitecture.WebAPI.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry Logging, Tracing, Metrics
#region OpenTelemetry
builder.Logging.ClearProviders();

var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
Uri? otlpUri = null;
if (!string.IsNullOrEmpty(otlpEndpoint))
    otlpUri = new Uri(otlpEndpoint);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("CleanArchitecture-WebApi"))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
               .AddHttpClientInstrumentation();
        if (otlpUri is not null)
            tracing.AddOtlpExporter(options =>
            {
                options.Endpoint = otlpUri;
                options.Protocol = OtlpExportProtocol.Grpc;
            });
    })
    .WithMetrics(metric =>
    {
        metric.AddConsoleExporter();
        if (otlpUri is not null)
            metric.AddOtlpExporter(options =>
            {
                options.Endpoint = otlpUri;
                options.Protocol = OtlpExportProtocol.Grpc;
            });
    })
    .WithLogging(logging =>
    {
        logging.AddConsoleExporter();
        if (otlpUri is not null)
            logging.AddOtlpExporter(options =>
            {
                options.Endpoint = otlpUri;
                options.Protocol = OtlpExportProtocol.Grpc;
            });
    });
#endregion

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Add Application Layer
builder.Services.AddApplication();

// Add Infrastructure Layers
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddSharedServices();

// ─── API Versioning ───────────────────────────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version")
    );
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ─── Rate Limiting ────────────────────────────────────────────────────────────
var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
builder.Services.AddRateLimiter(options =>
{
    // Global fixed window policy
    options.AddFixedWindowLimiter("fixed", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromSeconds(
            rateLimitConfig.GetValue("WindowSeconds", 60));
        limiterOptions.PermitLimit =
            rateLimitConfig.GetValue("PermitLimit", 100);
        limiterOptions.QueueLimit =
            rateLimitConfig.GetValue("QueueLimit", 0);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Strict policy for auth endpoints (prevent brute-force)
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromSeconds(
            rateLimitConfig.GetValue("Auth:WindowSeconds", 60));
        limiterOptions.PermitLimit =
            rateLimitConfig.GetValue("Auth:PermitLimit", 10);
        limiterOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                ? (int)retryAfter.TotalSeconds : 60
        }, token);
    };
});

// ─── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ─── Output Cache ────────────────────────────────────────────────────────────
builder.Services.AddOutputCache(options =>
{
    // Default: cache GET responses for 60 seconds
    options.AddBasePolicy(policy => policy
        .Expire(TimeSpan.FromSeconds(60))
        .With(req => req.HttpContext.Request.Method == "GET")
        .Tag("all"));

    // Named policy for products list (shorter TTL)
    options.AddPolicy("products", policy => policy
        .Expire(TimeSpan.FromSeconds(30))
        .SetVaryByQuery("pageNumber", "pageSize", "status", "searchTerm")
        .Tag("products"));
});

// ─── Hangfire ─────────────────────────────────────────────────────────────────
var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnection")
    ?? throw new InvalidOperationException("Hangfire connection string not found.");

if (!builder.Environment.IsEnvironment("Testing"))
{
    // Ensure Hangfire database exists before Hangfire tries to use it.
    var builderObj = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(hangfireConnectionString);
    var dbName = builderObj.InitialCatalog;
    builderObj.InitialCatalog = "master";
    using (var connection = new Microsoft.Data.SqlClient.SqlConnection(builderObj.ConnectionString))
    {
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{dbName}') CREATE DATABASE [{dbName}]";
        command.ExecuteNonQuery();
    }

    builder.Services.AddHangfire(configuration => configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

    builder.Services.AddHangfireServer();
}

// ─── Health Checks ────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("Database")
    .AddDbContextCheck<IdentityDbContext>("Identity Database");

var app = builder.Build();

// ─── Middleware Pipeline ──────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    // Scalar API Reference UI (via CDN — avoids NuGet bundle version conflicts)
    app.MapGet("/scalar/{documentName}", (string documentName) =>
    {
        var specUrl = $"/swagger/{documentName}/swagger.json";
        var html = """
            <!doctype html>
            <html lang="en">
            <head>
                <title>Clean Architecture API</title>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <style>body { margin: 0; }</style>
            </head>
            <body>
                <div id="app"></div>
                <script src="/js/scalar.min.js"></script>
                <script>
                    Scalar.createApiReference('#app', {
                        url: '__SPEC_URL__',
                        theme: 'purple',
                        defaultHttpClient: { targetKey: 'c#', clientKey: 'httpclient' },
                        authentication: { preferredSecurityScheme: 'Bearer' }
                    })
                </script>
            </body>
            </html>
            """.Replace("__SPEC_URL__", specUrl);
        return Results.Content(html, "text/html");
    });
}

// Redirect root to Scalar UI
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/scalar/v1");
        return;
    }
    await next();
});

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHangfireDashboard("/hangfire");
}

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            })
        });
        await context.Response.WriteAsync(result);
    }
});

try
{
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Seed Roles
        var roles = new[] { "Admin", "Member" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed default admin
        if (await userManager.FindByEmailAsync("admin@localhost") == null)
        {
            var admin = new ApplicationUser { UserName = "admin@localhost", Email = "admin@localhost", FullName = "System Administrator" };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        // Seed default member
        if (await userManager.FindByEmailAsync("member@localhost") == null)
        {
            var member = new ApplicationUser { UserName = "member@localhost", Email = "member@localhost", FullName = "Regular Member" };
            var result = await userManager.CreateAsync(member, "Member123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(member, "Member");
            }
        }
    }

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting Clean Architecture Web API");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application terminated unexpectedly: {ex}");
}

public partial class Program { }
