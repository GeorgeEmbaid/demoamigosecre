using AmigoSecreto.Models;

namespace AmigoSecreto.Services;

public interface IPersonService
{
    Task<List<Person>> GetAllAsync();
    Task<List<Person>> SearchPeopleAsync(string? searchTerm);
    Task<Person?> GetByIdAsync(int id);
    Task<Person?> AuthenticateParticipantAsync(string username, string password);
    Task<Person> ChangePasswordAsync(int personId, string newPassword);
    Task<Person> CreateAsync(Person person);
    Task<Person> UpdateAsync(Person person);
    Task<bool> DeleteAsync(int id);
    Task<int> GetCountAsync();
    Task ResetSampleDataAsync();
}
