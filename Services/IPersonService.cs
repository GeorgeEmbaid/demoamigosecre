using AmigoSecreto.Models;

namespace AmigoSecreto.Services;

public interface IPersonService
{
    Task<List<Person>> GetAllAsync();
    Task<List<Person>> SearchPeopleAsync(string? searchTerm);
    Task<Person?> GetByIdAsync(int id);
    Task<Person> CreateAsync(Person person);
    Task<Person> UpdateAsync(Person person);
    Task<bool> DeleteAsync(int id);
    Task<int> GetCountAsync();
    Task ResetSampleDataAsync();
}
