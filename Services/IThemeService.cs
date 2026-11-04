using Microsoft.JSInterop;

namespace AmigoSecreto.Services;

public interface IThemeService
{
    bool IsDarkMode { get; }
    event Action? OnThemeChanged;
    Task InitializeAsync(IJSRuntime js);
    Task<bool> ToggleThemeAsync(IJSRuntime js);
    Task SetDarkModeAsync(IJSRuntime js, bool isDark);
}
