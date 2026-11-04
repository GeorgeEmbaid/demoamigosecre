using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace AmigoSecreto.Services;

public class ThemeService : IThemeService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool _isDarkMode;
    private bool _initialized;

    public bool IsDarkMode => _isDarkMode;
    public event Action? OnThemeChanged;

    public ThemeService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        try
        {
            var cookie = _httpContextAccessor.HttpContext?.Request.Cookies["app_theme"];
            if (cookie == "dark")
            {
                _isDarkMode = true;
            }
        }
        catch
        {
            // Seguro ante contextos donde HttpContext no esté enlazado
        }
    }

    public async Task InitializeAsync(IJSRuntime js)
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            var theme = await js.InvokeAsync<string>("themeManager.getTheme");
            bool isDark = theme == "dark";
            if (_isDarkMode != isDark)
            {
                _isDarkMode = isDark;
                OnThemeChanged?.Invoke();
            }
        }
        catch
        {
            // Seguro durante prerenderizado o desconexión transitoria
        }
    }

    public async Task<bool> ToggleThemeAsync(IJSRuntime js)
    {
        _isDarkMode = !_isDarkMode;
        var theme = _isDarkMode ? "dark" : "light";
        try
        {
            await js.InvokeAsync<string>("themeManager.setTheme", theme);
        }
        catch
        {
        }
        OnThemeChanged?.Invoke();
        return _isDarkMode;
    }

    public async Task SetDarkModeAsync(IJSRuntime js, bool isDark)
    {
        if (_isDarkMode == isDark) return;
        _isDarkMode = isDark;
        var theme = _isDarkMode ? "dark" : "light";
        try
        {
            await js.InvokeAsync<string>("themeManager.setTheme", theme);
        }
        catch
        {
        }
        OnThemeChanged?.Invoke();
    }
}
