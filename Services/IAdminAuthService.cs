using AmigoSecreto.Models;

namespace AmigoSecreto.Services;

public interface IAdminAuthService
{
    // Estado del Administrador (admin / admin2026) - Separado de los participantes
    bool IsAuthenticated { get; }
    string? CurrentUser { get; }

    // Estado del Participante autenticado en el portal principal
    Person? CurrentParticipant { get; }
    bool IsParticipantAuthenticated { get; }

    event Action? OnAuthenticationStateChanged;

    bool Login(string username, string password);
    void Logout();

    void SetParticipant(Person? participant);
    void LogoutParticipant();
}
