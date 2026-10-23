using Microsoft.JSInterop;
using MudBlazor;
using CleanArchitecture.WebUI.Components.Layout;

namespace CleanArchitecture.WebUI.Services;

public class ThemeStateService
{
    private readonly IJSRuntime _js;
    private const string PresetStorageKey = "cleanarch_theme_preset";
    private const string DarkModeStorageKey = "cleanarch_theme_dark";

    public ThemePresetId CurrentPreset { get; private set; } = ThemePresetId.Indigo;
    public bool IsDarkMode { get; private set; } = false;
    public MudTheme CurrentTheme { get; private set; } = AppTheme.CreateTheme(ThemePresetId.Indigo);
    public ThemePresetInfo CurrentPresetInfo => AppTheme.GetPreset(CurrentPreset);

    public event Action? OnChange;

    public ThemeStateService(IJSRuntime js)
    {
        _js = js;
        CurrentTheme.LayoutProperties.AppbarHeight = "72px";
    }

    public async Task InitializeAsync()
    {
        try
        {
            var savedPreset = await _js.InvokeAsync<string?>("localStorage.getItem", PresetStorageKey);
            if (!string.IsNullOrEmpty(savedPreset) && Enum.TryParse<ThemePresetId>(savedPreset, true, out var presetId))
            {
                CurrentPreset = presetId;
            }

            var savedDark = await _js.InvokeAsync<string?>("localStorage.getItem", DarkModeStorageKey);
            if (!string.IsNullOrEmpty(savedDark) && bool.TryParse(savedDark, out var isDark))
            {
                IsDarkMode = isDark;
            }

            RebuildTheme();
            await SyncBrowserAsync();
            OnChange?.Invoke();
        }
        catch
        {
            // Prerender or browser security fallback
            RebuildTheme();
        }
    }

    public async Task SetPresetAsync(ThemePresetId presetId)
    {
        if (CurrentPreset == presetId) return;

        CurrentPreset = presetId;
        RebuildTheme();

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", PresetStorageKey, presetId.ToString());
            await SyncBrowserAsync();
        }
        catch { }

        OnChange?.Invoke();
    }

    public async Task SetDarkModeAsync(bool isDark)
    {
        IsDarkMode = isDark;

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", DarkModeStorageKey, isDark.ToString());
            await SyncBrowserAsync();
        }
        catch { }

        OnChange?.Invoke();
    }

    public Task ToggleDarkModeAsync() => SetDarkModeAsync(!IsDarkMode);

    private void RebuildTheme()
    {
        CurrentTheme = AppTheme.CreateTheme(CurrentPreset);
        CurrentTheme.LayoutProperties.AppbarHeight = "72px";
    }

    private async Task SyncBrowserAsync()
    {
        try
        {
            var info = CurrentPresetInfo;
            var primaryRgb = IsDarkMode ? info.DarkPrimaryRgb : info.PrimaryRgb;
            var gradient = IsDarkMode ? info.DarkPrimaryGradient : info.PrimaryGradient;
            var themeClass = $"theme-{CurrentPreset.ToString().ToLowerInvariant()}";

            await _js.InvokeVoidAsync("applyThemeConfiguration", new
            {
                isDark = IsDarkMode,
                primaryRgb = primaryRgb,
                gradient = gradient,
                themeClass = themeClass
            });
        }
        catch { }
    }
}
