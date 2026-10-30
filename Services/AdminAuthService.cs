namespace AmigoSecreto.Services;

public class AdminAuthService : IAdminAuthService
{
    private const string ExpectedUser = "admin";
    private const string ExpectedPass = "bnc2026";

    public bool IsAuthenticated { get; private set; }
    public string? CurrentUser { get; private set; }

    public event Action? OnAuthenticationStateChanged;

    public bool Login(string username, string password)
    {
        if (string.Equals(username?.Trim(), ExpectedUser, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(password?.Trim(), ExpectedPass, StringComparison.Ordinal))
        {
            IsAuthenticated = true;
            CurrentUser = "Administrador BNC";
            OnAuthenticationStateChanged?.Invoke();
            return true;
        }

        IsAuthenticated = false;
        CurrentUser = null;
        OnAuthenticationStateChanged?.Invoke();
        return false;
    }

    public void Logout()
    {
        IsAuthenticated = false;
        CurrentUser = null;
        OnAuthenticationStateChanged?.Invoke();
    }
}
