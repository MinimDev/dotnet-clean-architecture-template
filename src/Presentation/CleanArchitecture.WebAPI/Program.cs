using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CleanArchitecture.Application;
using CleanArchitecture.Infrastructure.Identity;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Infrastructure.Shared;
using CleanArchitecture.WebAPI.Middleware;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.SqlServer;

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
builder.Services.AddSwaggerGen(options =>
{
    // One SwaggerDoc per API version
    var provider = builder.Services.BuildServiceProvider()
        .GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Clean Architecture API",
            Version = description.ApiVersion.ToString(),
            Description = description.IsDeprecated
                ? "This API version is deprecated."
                : "A Clean Architecture template API with CQRS, MediatR, and JWT Authentication"
        });
    }

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

    var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Clean Architecture API")
            .WithTheme(ScalarTheme.Purple)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .WithEndpointPrefix("/scalar/{documentName}")
            .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
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

app.UseHttpsRedirection();
app.UseCors();
app.UseRateLimiter();
app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

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
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Starting Clean Architecture Web API");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application terminated unexpectedly: {ex}");
}

public partial class Program { }
