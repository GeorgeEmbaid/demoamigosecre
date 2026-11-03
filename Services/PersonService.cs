using System.Globalization;
using System.Text;
using AmigoSecreto.Data;
using AmigoSecreto.Models;
using Microsoft.EntityFrameworkCore;

namespace AmigoSecreto.Services;

public class PersonService : IPersonService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public PersonService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Person>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.People
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();
    }

    public async Task<List<Person>> SearchPeopleAsync(string? searchTerm)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.People.AsQueryable();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await query
                .OrderBy(p => p.FirstName)
                .ThenBy(p => p.LastName)
                .ToListAsync();
        }

        // Normalizamos los términos de búsqueda para garantizar compatibilidad case-insensitive y acentos
        var rawTerm = searchTerm.Trim();
        var normalizedTerm = RemoveDiacritics(rawTerm).ToLowerInvariant();
        var tokens = normalizedTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Traemos los candidatos de la base de datos y filtramos con normalización precisa
        var list = await query.ToListAsync();

        return list.Where(p =>
        {
            var normFirst = RemoveDiacritics(p.FirstName ?? "").ToLowerInvariant();
            var normLast = RemoveDiacritics(p.LastName ?? "").ToLowerInvariant();
            var normFull = $"{normFirst} {normLast}".Trim();
            var normFullReverse = $"{normLast} {normFirst}".Trim();

            // Debe coincidir con todos los tokens buscados (ej: "carlos men" encuentra a "Carlos Mendoza")
            return tokens.All(t => 
                normFirst.Contains(t) || 
                normLast.Contains(t) || 
                normFull.Contains(t) || 
                normFullReverse.Contains(t));
        })
        .OrderBy(p => p.FirstName)
        .ThenBy(p => p.LastName)
        .ToList();
    }

    public async Task<Person?> GetByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.People.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Person> CreateAsync(Person person)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        person.CreatedAt = DateTime.UtcNow;
        person.UpdatedAt = DateTime.UtcNow;
        context.People.Add(person);
        await context.SaveChangesAsync();
        return person;
    }

    public async Task<Person> UpdateAsync(Person person)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existing = await context.People.FirstOrDefaultAsync(p => p.Id == person.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"No se encontró la persona con Id {person.Id}");
        }

        existing.FirstName = person.FirstName.Trim();
        existing.LastName = person.LastName.Trim();
        existing.Email = person.Email?.Trim();
        existing.ShirtSize = person.ShirtSize?.Trim();
        existing.ShoeSize = person.ShoeSize?.Trim();
        existing.FavoriteColor = person.FavoriteColor?.Trim();
        existing.AvatarColor = person.AvatarColor ?? "#002855";
        existing.Likes = person.Likes.Trim();
        existing.Dislikes = person.Dislikes.Trim();
        existing.Allergies = person.Allergies?.Trim() ?? string.Empty;
        existing.Notes = person.Notes?.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var person = await context.People.FirstOrDefaultAsync(p => p.Id == id);
        if (person == null) return false;

        context.People.Remove(person);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetCountAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.People.CountAsync();
    }

    public async Task ResetSampleDataAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.People.RemoveRange(context.People);
        await context.SaveChangesAsync();
        await DbInitializer.InitializeAsync(context, forceReload: true);
    }

    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
