using MudBlazor;

namespace CleanArchitecture.WebUI.Components.Layout;

public enum ThemePresetId
{
    Indigo,
    ArcticMist,
    ForestWalk,
    DesertBloom,
    MochaMood,
    NeutralStone
}

public class ThemePresetInfo
{
    public ThemePresetId Id { get; init; }
    public string Name { get; init; } = "";
    public string Category { get; init; } = "";
    public string Description { get; init; } = "";
    public string[] SwatchColors { get; init; } = Array.Empty<string>();
    public PaletteLight LightPalette { get; init; } = new();
    public PaletteDark DarkPalette { get; init; } = new();
    public string PrimaryGradient { get; init; } = "";
    public string DarkPrimaryGradient { get; init; } = "";
    public string PrimaryRgb { get; init; } = "";
    public string DarkPrimaryRgb { get; init; } = "";
}

public static class AppTheme
{
    public static readonly Typography Typography = new()
    {
        Default = new DefaultTypography()
        {
            FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" },
            FontSize = "0.85rem",
            LetterSpacing = "0.01em",
            LineHeight = "1.45"
        },
        H1 = new H1Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontSize = "1.5rem", FontWeight = "700", LineHeight = "1.2" },
        H2 = new H2Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontSize = "1.35rem", FontWeight = "700", LineHeight = "1.25" },
        H3 = new H3Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontSize = "1.2rem", FontWeight = "700", LineHeight = "1.3" },
        H4 = new H4Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontSize = "1.05rem", FontWeight = "600", LineHeight = "1.35" },
        H5 = new H5Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontSize = "0.925rem", FontWeight = "600", LineHeight = "1.4" },
        H6 = new H6Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontSize = "0.85rem", FontWeight = "600", LineHeight = "1.4" },
        Subtitle1 = new Subtitle1Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "0.8125rem", FontWeight = "500", LineHeight = "1.4" },
        Subtitle2 = new Subtitle2Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "0.78rem", FontWeight = "600", LineHeight = "1.4" },
        Body1 = new Body1Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "0.85rem", LineHeight = "1.45" },
        Body2 = new Body2Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "0.78rem", LineHeight = "1.45" },
        Caption = new CaptionTypography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "0.72rem", LineHeight = "1.3" },
        Button = new ButtonTypography { FontFamily = new[] { "Inter", "sans-serif" }, FontSize = "0.8rem", FontWeight = "600", TextTransform = "none" }
    };

    public static readonly Dictionary<ThemePresetId, ThemePresetInfo> Presets = new()
    {
        [ThemePresetId.Indigo] = new ThemePresetInfo
        {
            Id = ThemePresetId.Indigo,
            Name = "CleanArch Enterprise Blue",
            Category = "enterprise - saas",
            Description = "Authoritative enterprise cobalt & slate blue palette",
            SwatchColors = new[] { "#1E40AF", "#2563EB", "#3B82F6", "#60A5FA", "#EFF6FF" },
            PrimaryGradient = "linear-gradient(135deg, #1d4ed8 0%, #2563eb 100%)",
            DarkPrimaryGradient = "linear-gradient(135deg, #3b82f6 0%, #60a5fa 100%)",
            PrimaryRgb = "37, 99, 235",
            DarkPrimaryRgb = "59, 130, 246",
            LightPalette = CreateLightPalette("#2563eb", "#1d4ed8", "#3b82f6", "#64748b"),
            DarkPalette = CreateDarkPalette("#3b82f6", "#2563eb", "#60a5fa", "#94a3b8")
        },
        [ThemePresetId.ArcticMist] = new ThemePresetInfo
        {
            Id = ThemePresetId.ArcticMist,
            Name = "Arctic Mist",
            Category = "cool - winter",
            Description = "Crisp cyan & deep oceanic blue for high clarity and cool atmosphere",
            SwatchColors = new[] { "#CAF0F8", "#90E0EF", "#00B4D8", "#0077B6", "#03045E" },
            PrimaryGradient = "linear-gradient(135deg, #0077b6 0%, #00b4d8 100%)",
            DarkPrimaryGradient = "linear-gradient(135deg, #38bdf8 0%, #00b4d8 100%)",
            PrimaryRgb = "0, 119, 182",
            DarkPrimaryRgb = "56, 189, 248",
            LightPalette = CreateLightPalette("#0077b6", "#03045e", "#00b4d8", "#64748b"),
            DarkPalette = CreateDarkPalette("#38bdf8", "#0284c7", "#7dd3fc", "#94a3b8")
        },
        [ThemePresetId.ForestWalk] = new ThemePresetInfo
        {
            Id = ThemePresetId.ForestWalk,
            Name = "Forest Walk",
            Category = "nature - earthy",
            Description = "Earthy moss olive & warm golden sand for rich organic balance",
            SwatchColors = new[] { "#283618", "#606C38", "#FEFAE0", "#DDA15E", "#BC6C25" },
            PrimaryGradient = "linear-gradient(135deg, #283618 0%, #606c38 100%)",
            DarkPrimaryGradient = "linear-gradient(135deg, #4ade80 0%, #86efac 100%)",
            PrimaryRgb = "40, 54, 24",
            DarkPrimaryRgb = "74, 222, 128",
            LightPalette = CreateLightPalette("#283618", "#1b2610", "#606c38", "#bc6c25"),
            DarkPalette = CreateDarkPalette("#4ade80", "#22c55e", "#86efac", "#dda15e")
        },
        [ThemePresetId.DesertBloom] = new ThemePresetInfo
        {
            Id = ThemePresetId.DesertBloom,
            Name = "Desert Bloom",
            Category = "warm - earthy",
            Description = "Vibrant sunset terracotta and deep ocean teal accents",
            SwatchColors = new[] { "#F4A261", "#E76F51", "#2A9D8F", "#264653", "#E9C46A" },
            PrimaryGradient = "linear-gradient(135deg, #e76f51 0%, #f4a261 100%)",
            DarkPrimaryGradient = "linear-gradient(135deg, #f87171 0%, #f4a261 100%)",
            PrimaryRgb = "231, 111, 81",
            DarkPrimaryRgb = "248, 113, 113",
            LightPalette = CreateLightPalette("#e76f51", "#d65a3d", "#f4a261", "#2a9d8f"),
            DarkPalette = CreateDarkPalette("#f87171", "#ef4444", "#fca5a5", "#2dd4bf")
        },
        [ThemePresetId.MochaMood] = new ThemePresetInfo
        {
            Id = ThemePresetId.MochaMood,
            Name = "Mocha Mood",
            Category = "warm - earthy",
            Description = "Rich espresso, roasted taupe, and warm latte aesthetic",
            SwatchColors = new[] { "#3E2723", "#5D4037", "#8D6E63", "#D7CCC8", "#EFEBE9" },
            PrimaryGradient = "linear-gradient(135deg, #5d4037 0%, #8d6e63 100%)",
            DarkPrimaryGradient = "linear-gradient(135deg, #d7ccc8 0%, #bcaaa4 100%)",
            PrimaryRgb = "93, 64, 55",
            DarkPrimaryRgb = "215, 204, 200",
            LightPalette = CreateLightPalette("#5d4037", "#3e2723", "#8d6e63", "#8d6e63"),
            DarkPalette = CreateDarkPalette("#d7ccc8", "#bcaaa4", "#efebe9", "#a1887f")
        },
        [ThemePresetId.NeutralStone] = new ThemePresetInfo
        {
            Id = ThemePresetId.NeutralStone,
            Name = "Neutral Stone",
            Category = "neutral - minimal",
            Description = "Pure monochrome charcoal and minimalist stone tones",
            SwatchColors = new[] { "#F5F5F4", "#E7E5E4", "#A8A29E", "#57534E", "#1C1917" },
            PrimaryGradient = "linear-gradient(135deg, #1c1917 0%, #57534e 100%)",
            DarkPrimaryGradient = "linear-gradient(135deg, #e2e8f0 0%, #94a3b8 100%)",
            PrimaryRgb = "28, 25, 23",
            DarkPrimaryRgb = "226, 232, 240",
            LightPalette = CreateLightPalette("#1c1917", "#0c0a09", "#57534e", "#78716c"),
            DarkPalette = CreateDarkPalette("#e2e8f0", "#cbd5e1", "#f8fafc", "#94a3b8")
        }
    };

    private static PaletteLight CreateLightPalette(string primary, string primaryDarken, string primaryLighten, string secondary) => new()
    {
        Primary = primary,
        PrimaryDarken = primaryDarken,
        PrimaryLighten = primaryLighten,
        Secondary = secondary,
        Black = "#0f172a",
        AppbarText = "#1e293b",
        AppbarBackground = "rgba(255, 255, 255, 0.85)",
        DrawerBackground = "#ffffff",
        GrayLight = "#f1f5f9",
        GrayLighter = "#f8fafc",
        Background = "#f8fafc",
        Surface = "#ffffff",
        Success = "#10b981",
        Warning = "#f59e0b",
        Error = "#ef4444",
        Info = "#3b82f6",
        BackgroundGray = "#f1f5f9",
        LinesDefault = "#e2e8f0",
        TableLines = "#e2e8f0",
        Divider = "#e2e8f0"
    };

    private static PaletteDark CreateDarkPalette(string primary, string primaryDarken, string primaryLighten, string secondary) => new()
    {
        Primary = primary,
        PrimaryDarken = primaryDarken,
        PrimaryLighten = primaryLighten,
        Secondary = secondary,
        Surface = "#111827",
        Background = "#090d16",
        BackgroundGray = "#0f172a",
        AppbarText = "#f8fafc",
        AppbarBackground = "rgba(11, 15, 25, 0.85)",
        DrawerBackground = "#0d131f",
        ActionDefault = "#94a3b8",
        ActionDisabled = "#475569",
        ActionDisabledBackground = "#1f293d",
        TextPrimary = "#f1f5f9",
        TextSecondary = "#94a3b8",
        TextDisabled = "#64748b",
        DrawerIcon = "#cbd5e1",
        DrawerText = "#f1f5f9",
        GrayLight = "#1f293d",
        GrayLighter = "#111827",
        Info = "#38bdf8",
        Success = "#34d399",
        Warning = "#fbbf24",
        Error = "#f87171",
        LinesDefault = "rgba(255, 255, 255, 0.08)",
        TableLines = "rgba(255, 255, 255, 0.06)",
        Divider = "rgba(255, 255, 255, 0.08)",
        OverlayLight = "#090d1680"
    };

    public static ThemePresetInfo GetPreset(ThemePresetId presetId) =>
        Presets.TryGetValue(presetId, out var info) ? info : Presets[ThemePresetId.Indigo];

    public static IEnumerable<ThemePresetInfo> GetAllPresets() => Presets.Values;

    public static MudTheme CreateTheme(ThemePresetId presetId = ThemePresetId.Indigo)
    {
        var preset = GetPreset(presetId);
        return new MudTheme()
        {
            PaletteLight = preset.LightPalette,
            PaletteDark = preset.DarkPalette,
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "12px",
            },
            Typography = Typography
        };
    }
}
