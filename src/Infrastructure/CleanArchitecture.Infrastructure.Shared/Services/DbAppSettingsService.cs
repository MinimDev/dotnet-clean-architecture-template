using System.ComponentModel;
using System.Globalization;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Features.AppSettings.DTOs;
using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Infrastructure.Shared.Services;

/// <summary>
/// Implementasi IAppSettingsService menggunakan Database (IApplicationDbContext)
/// dengan in-memory caching (IMemoryCache) dan fallback ke IConfiguration.
/// </summary>
public class DbAppSettingsService : IAppSettingsService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DbAppSettingsService> _logger;

    private static readonly TimeSpan DefaultSlidingExpiration = TimeSpan.FromHours(1);
    private static readonly TimeSpan DefaultAbsoluteExpiration = TimeSpan.FromHours(24);

    public DbAppSettingsService(
        IServiceScopeFactory scopeFactory,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<DbAppSettingsService> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    private static string GetCacheKey(string key) => $"appsettings:{key.Trim().ToLowerInvariant()}";

    public async Task<string?> GetAsync(string key, string? defaultValue = null, CancellationToken ct = default)
    {
        var cacheKey = GetCacheKey(key);

        if (_cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var setting = await context.AppSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key.ToLower() == key.ToLower(), ct);

            if (setting != null)
            {
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    SlidingExpiration = DefaultSlidingExpiration,
                    AbsoluteExpirationRelativeToNow = DefaultAbsoluteExpiration
                };
                _cache.Set(cacheKey, setting.Value, cacheOptions);
                return setting.Value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading setting '{Key}' from database. Falling back to IConfiguration.", key);
        }

        // Fallback ke IConfiguration (appsettings.json atau Environment Variables)
        var configValue = _configuration[key];
        if (configValue != null)
        {
            return configValue;
        }

        return defaultValue;
    }

    public async Task<T?> GetAsync<T>(string key, T? defaultValue = default, CancellationToken ct = default)
    {
        var rawValue = await GetAsync(key, null, ct);
        if (rawValue == null)
        {
            return defaultValue;
        }

        try
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (targetType == typeof(bool))
            {
                if (bool.TryParse(rawValue, out var boolResult))
                    return (T)(object)boolResult;
            }

            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                var converted = converter.ConvertFrom(null, CultureInfo.InvariantCulture, rawValue);
                if (converted != null)
                    return (T)converted;
            }

            return (T)Convert.ChangeType(rawValue, targetType, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert setting '{Key}' value '{Value}' to type {Type}. Returning default.", key, rawValue, typeof(T).Name);
            return defaultValue;
        }
    }

    public async Task SetAsync(string key, string value, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var setting = await context.AppSettings
            .FirstOrDefaultAsync(s => s.Key.ToLower() == key.ToLower(), ct);

        if (setting == null)
        {
            // Jika belum ada, buat sebagai dynamic string setting
            setting = AppSetting.Create(key, value, "string", null, "General", isReadOnly: false);
            context.AppSettings.Add(setting);
        }
        else
        {
            setting.UpdateValue(value);
        }

        await context.SaveChangesAsync(ct);

        // Update in-memory cache
        var cacheKey = GetCacheKey(key);
        var cacheOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = DefaultSlidingExpiration,
            AbsoluteExpirationRelativeToNow = DefaultAbsoluteExpiration
        };
        _cache.Set(cacheKey, value, cacheOptions);
    }

    public async Task<bool> IsFeatureEnabledAsync(string featureKey, CancellationToken ct = default)
    {
        var isEnabled = await GetAsync<bool>(featureKey, defaultValue: false, ct);
        return isEnabled;
    }

    public async Task<IReadOnlyList<AppSettingDto>> GetAllAsync(string? group = null, CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var query = context.AppSettings.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(group))
        {
            query = query.Where(s => s.Group.ToLower() == group.ToLower());
        }

        return await query
            .OrderBy(s => s.Group)
            .ThenBy(s => s.Key)
            .Select(s => new AppSettingDto
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value,
                DataType = s.DataType,
                Description = s.Description,
                Group = s.Group,
                IsReadOnly = s.IsReadOnly,
                ModifiedAt = s.ModifiedAt ?? s.CreatedAt,
                ModifiedBy = s.ModifiedBy ?? s.CreatedBy
            })
            .ToListAsync(ct);
    }
}
