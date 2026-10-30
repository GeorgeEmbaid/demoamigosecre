using AmigoSecreto.Models;

namespace AmigoSecreto.Services;

public interface IPersonService
{
    Task<List<Person>> GetAllAsync();
    Task<List<Person>> SearchPeopleAsync(string? searchTerm, string? department = null);
    Task<Person?> GetByIdAsync(int id);
    Task<Person> CreateAsync(Person person);
    Task<Person> UpdateAsync(Person person);
    Task<bool> DeleteAsync(int id);
    Task<List<string>> GetDepartmentsAsync();
    Task<int> GetCountAsync();
    Task ResetSampleDataAsync();
}
