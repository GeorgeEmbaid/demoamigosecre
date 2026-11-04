using AmigoSecreto.Models;

namespace AmigoSecreto.Services;

public class AdminAuthService : IAdminAuthService
{
    private const string ExpectedUser = "admin";
    private const string ExpectedPass = "admin2026";
    private const string LegacyPass = "bnc2026";

    public bool IsAuthenticated { get; private set; }
    public string? CurrentUser { get; private set; }

    public Person? CurrentParticipant { get; private set; }
    public bool IsParticipantAuthenticated => CurrentParticipant != null;

    public event Action? OnAuthenticationStateChanged;

    public bool Login(string username, string password)
    {
        var cleanPass = password?.Trim();
        if (string.Equals(username?.Trim(), ExpectedUser, StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(cleanPass, ExpectedPass, StringComparison.Ordinal) || string.Equals(cleanPass, LegacyPass, StringComparison.Ordinal)))
        {
            IsAuthenticated = true;
            CurrentUser = "Administrador";
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

    public void SetParticipant(Person? participant)
    {
        CurrentParticipant = participant;
        OnAuthenticationStateChanged?.Invoke();
    }

    public void LogoutParticipant()
    {
        CurrentParticipant = null;
        OnAuthenticationStateChanged?.Invoke();
    }
}
