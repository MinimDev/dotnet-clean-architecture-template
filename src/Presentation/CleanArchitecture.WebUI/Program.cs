using MudBlazor.Services;
using CleanArchitecture.WebUI.Services;
using CleanArchitecture.WebUI.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenTelemetry Logging, Tracing, Metrics
#region OpenTelemetry
builder.Logging.ClearProviders();

var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"];
Uri? otlpUri = null;
if (!string.IsNullOrEmpty(otlpEndpoint))
    otlpUri = new Uri(otlpEndpoint);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("CleanArchitecture-WebUI"))
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

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
var httpClientBuilder = builder.Services.AddHttpClient("WebAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7253");
});

// In Development: bypass SSL certificate validation for localhost dev certificate.
// In Production: standard SSL validation applies — a valid trusted certificate is required.
if (builder.Environment.IsDevelopment())
{
    httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Bypass SSL validation for development only (localhost dev certificate is self-signed).
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
}

builder.Services.AddScoped<TokenProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthClient>();
builder.Services.AddScoped<ProductsClient>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
