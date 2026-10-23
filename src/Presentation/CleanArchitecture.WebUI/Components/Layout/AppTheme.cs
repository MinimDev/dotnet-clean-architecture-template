using MudBlazor;

namespace CleanArchitecture.WebUI.Components.Layout;

public static class AppTheme
{
    public static readonly PaletteLight LightPalette = new()
    {
        Primary = "#4f46e5",
        PrimaryDarken = "#4338ca",
        PrimaryLighten = "#818cf8",
        Secondary = "#64748b",
        Black = "#0f172a",
        AppbarText = "#1e293b",
        AppbarBackground = "rgba(255,255,255,0.85)",
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
        Divider = "#e2e8f0"
    };

    public static readonly PaletteDark DarkPalette = new()
    {
        Primary = "#818cf8",
        PrimaryLighten = "#a5b4fc",
        PrimaryDarken = "#4f46e5",
        Secondary = "#94a3b8",
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
        LinesDefault = "#1f293d",
        TableLines = "#1f293d",
        Divider = "#1f293d",
        OverlayLight = "#090d1680",
    };

    public static readonly Typography Typography = new()
    {
        Default = new DefaultTypography()
        {
            FontFamily = new[] { "Inter", "Helvetica", "Arial", "sans-serif" },
            FontSize = "0.95rem",
            LetterSpacing = "0.01em"
        },
        H1 = new H1Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "700" },
        H2 = new H2Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "700" },
        H3 = new H3Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "700" },
        H4 = new H4Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "600" },
        H5 = new H5Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "600" },
        H6 = new H6Typography { FontFamily = new[] { "Outfit", "sans-serif" }, FontWeight = "600" },
        Button = new ButtonTypography { FontFamily = new[] { "Inter", "sans-serif" }, FontWeight = "600", TextTransform = "none" },
        Subtitle1 = new Subtitle1Typography { FontFamily = new[] { "Inter", "sans-serif" }, FontWeight = "500" }
    };

    public static MudTheme CreateTheme() => new()
    {
        PaletteLight = LightPalette,
        PaletteDark = DarkPalette,
        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "12px",
        },
        Typography = Typography
    };
}
