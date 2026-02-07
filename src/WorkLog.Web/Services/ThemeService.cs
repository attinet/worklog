using Blazored.LocalStorage;
using MudBlazor;
using WorkLog.Web.Shared;

namespace WorkLog.Web.Services;

/// <summary>
/// 主題管理服務
/// </summary>
public class ThemeService
{
    private const string THEME_KEY = "theme_preference";
    private readonly ILocalStorageService _localStorage;
    private bool _isDarkMode;
    
    public ThemeService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    /// <summary>
    /// 是否為深色模式
    /// </summary>
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                _ = SaveThemePreferenceAsync();
                OnThemeChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// 應用程式主題
    /// </summary>
    public MudTheme Theme => AppTheme.Theme;

    /// <summary>
    /// 主題變更事件
    /// </summary>
    public event Action? OnThemeChanged;

    /// <summary>
    /// 初始化主題服務（從本機儲存載入偏好設定）
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var savedTheme = await _localStorage.GetItemAsStringAsync(THEME_KEY);
            _isDarkMode = savedTheme == "dark";
        }
        catch
        {
            // 預設使用淺色模式
            _isDarkMode = false;
        }
    }

    /// <summary>
    /// 切換主題
    /// </summary>
    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
    }

    /// <summary>
    /// 儲存主題偏好設定到本機儲存
    /// </summary>
    private async Task SaveThemePreferenceAsync()
    {
        try
        {
            await _localStorage.SetItemAsStringAsync(THEME_KEY, _isDarkMode ? "dark" : "light");
        }
        catch
        {
            // 儲存失敗時靜默處理
        }
    }
}
