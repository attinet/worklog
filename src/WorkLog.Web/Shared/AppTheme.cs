using MudBlazor;

namespace WorkLog.Web.Shared;

/// <summary>
/// 應用程式主題定義
/// </summary>
public static class AppTheme
{
    private static readonly MudTheme _defaultTheme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#2563eb", // 藍色
            Secondary = "#7c3aed", // 紫色
            Tertiary = "#059669", // 綠色
            AppbarBackground = "#ffffff",
            AppbarText = "#1f2937",
            Background = "#f9fafb",
            BackgroundGray = "#f3f4f6",
            Surface = "#ffffff",
            DrawerBackground = "#ffffff",
            DrawerText = "#374151",
            DrawerIcon = "#6b7280",
            TextPrimary = "#111827",
            TextSecondary = "#6b7280",
            TextDisabled = "#9ca3af",
            ActionDefault = "#6b7280",
            ActionDisabled = "#d1d5db",
            ActionDisabledBackground = "#f3f4f6",
            Divider = "#e5e7eb",
            DividerLight = "#f3f4f6",
            TableLines = "#e5e7eb",
            LinesDefault = "#e5e7eb",
            LinesInputs = "#d1d5db",
            Success = "#10b981",
            Info = "#3b82f6",
            Warning = "#f59e0b",
            Error = "#ef4444",
            Dark = "#1f2937",
            HoverOpacity = 0.06,
            GrayDefault = "#9ca3af",
            GrayLight = "#e5e7eb",
            GrayLighter = "#f3f4f6",
            GrayDark = "#6b7280",
            GrayDarker = "#4b5563",
            OverlayLight = "rgba(255,255,255,0.5)",
        },
        
        PaletteDark = new PaletteDark
        {
            Primary = "#60a5fa", // 亮藍色
            Secondary = "#a78bfa", // 亮紫色
            Tertiary = "#34d399", // 亮綠色
            AppbarBackground = "#1e293b",
            AppbarText = "#f1f5f9",
            Background = "#0f172a",
            BackgroundGray = "#1e293b",
            Surface = "#1e293b",
            DrawerBackground = "#1e293b",
            DrawerText = "#cbd5e1",
            DrawerIcon = "#94a3b8",
            TextPrimary = "#f1f5f9",
            TextSecondary = "#94a3b8",
            TextDisabled = "#64748b",
            ActionDefault = "#94a3b8",
            ActionDisabled = "#475569",
            ActionDisabledBackground = "#334155",
            Divider = "#334155",
            DividerLight = "#1e293b",
            TableLines = "#334155",
            LinesDefault = "#334155",
            LinesInputs = "#475569",
            Success = "#10b981",
            Info = "#3b82f6",
            Warning = "#f59e0b",
            Error = "#ef4444",
            Dark = "#0f172a",
            HoverOpacity = 0.08,
            GrayDefault = "#64748b",
            GrayLight = "#334155",
            GrayLighter = "#1e293b",
            GrayDark = "#94a3b8",
            GrayDarker = "#cbd5e1",
            OverlayLight = "rgba(15,23,42,0.8)",
        },
        
        Shadows = new Shadow(),
        
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
            DrawerWidthLeft = "240px",
            DrawerWidthRight = "240px",
            AppbarHeight = "64px"
        },
        
        ZIndex = new ZIndex
        {
            Drawer = 1200,
            AppBar = 1100,
            Dialog = 1300,
            Popover = 1400,
            Snackbar = 1500,
            Tooltip = 1600
        }
    };

    /// <summary>
    /// 取得應用程式主題
    /// </summary>
    public static MudTheme Theme => _defaultTheme;
}
