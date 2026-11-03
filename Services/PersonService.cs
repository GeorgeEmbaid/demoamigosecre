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

        var list = await query.ToListAsync();

        return list.Where(p =>
        {
            var normFirst = RemoveDiacritics(p.FirstName ?? "").ToLowerInvariant();
            var normLast = RemoveDiacritics(p.LastName ?? "").ToLowerInvariant();
            var normFull = $"{normFirst} {normLast}".Trim();
            var normFullReverse = $"{normLast} {normFirst}".Trim();

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

    public async Task<Person?> AuthenticateParticipantAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var rawUser = username.Trim();
        // Si el usuario ingresa su correo completo (ej. gembaid@bncenlinea.net), extraer la parte del usuario
        if (rawUser.Contains('@'))
        {
            rawUser = rawUser.Split('@')[0].Trim();
        }

        var normUser = Person.NormalizeIdentifier(rawUser);
        var rawPass = password.Trim();
        var normPass = Person.NormalizeIdentifier(rawPass);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var allPeople = await context.People.ToListAsync();

        var candidate = allPeople.FirstOrDefault(p =>
            string.Equals(p.Username, rawUser, StringComparison.OrdinalIgnoreCase) ||
            Person.NormalizeIdentifier(p.Username) == normUser);

        if (candidate == null)
            return null;

        // Validar contraseña exacta o normalizada (sin acentos y case-insensitive)
        bool passwordValid =
            string.Equals(candidate.Password, rawPass, StringComparison.Ordinal) ||
            string.Equals(candidate.Password, rawPass, StringComparison.OrdinalIgnoreCase) ||
            Person.NormalizeIdentifier(candidate.Password) == normPass;

        return passwordValid ? candidate : null;
    }

    public async Task<Person> CreateAsync(Person person)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        person.FirstName = person.FirstName.Trim();
        person.LastName = person.LastName.Trim();

        if (string.IsNullOrWhiteSpace(person.Username))
        {
            person.Username = Person.GenerateDefaultUsername(person.FirstName, person.LastName);
        }
        else
        {
            person.Username = person.Username.Trim();
        }

        if (string.IsNullOrWhiteSpace(person.Password))
        {
            person.Password = Person.GenerateDefaultPassword(person.LastName);
        }
        else
        {
            person.Password = person.Password.Trim();
        }

        if (string.IsNullOrWhiteSpace(person.Email))
        {
            person.Email = $"{person.Username.ToLowerInvariant()}@bncenlinea.net";
        }
        else
        {
            person.Email = person.Email.Trim();
        }

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

        if (!string.IsNullOrWhiteSpace(person.Username))
        {
            existing.Username = person.Username.Trim();
        }
        if (!string.IsNullOrWhiteSpace(person.Password))
        {
            existing.Password = person.Password.Trim();
        }

        existing.Email = person.Email?.Trim();
        existing.ShirtSize = person.ShirtSize?.Trim();
        existing.ShoeSize = person.ShoeSize?.Trim();
        existing.FavoriteColor = person.FavoriteColor?.Trim();
        existing.AvatarColor = person.AvatarColor ?? "#002855";
        existing.ProfilePhotoPath = person.ProfilePhotoPath;
        existing.GiftImagePath = person.GiftImagePath;
        existing.GiftReferenceUrl = person.GiftReferenceUrl?.Trim();
        existing.Likes = person.Likes?.Trim() ?? string.Empty;
        existing.Dislikes = person.Dislikes?.Trim() ?? string.Empty;
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
