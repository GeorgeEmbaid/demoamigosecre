namespace AmigoSecreto.Services;

public interface IAdminAuthService
{
    bool IsAuthenticated { get; }
    string? CurrentUser { get; }
    event Action? OnAuthenticationStateChanged;

    bool Login(string username, string password);
    void Logout();
}
