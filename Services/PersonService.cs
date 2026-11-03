using System.Globalization;
using System.Text;
using System.Text.Json;
using AmigoSecreto.Data;
using AmigoSecreto.Models;
using Microsoft.EntityFrameworkCore;

namespace AmigoSecreto.Services;

public class PersonService : IPersonService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IImageStorageService _imageStorage;
    private readonly string _backupFilePath;

    public PersonService(
        IDbContextFactory<AppDbContext> contextFactory,
        IImageStorageService imageStorage,
        IWebHostEnvironment env)
    {
        _contextFactory = contextFactory;
        _imageStorage = imageStorage;

        var storageDir = Path.Combine(env.ContentRootPath, "ImagenesParticipantes");
        Directory.CreateDirectory(storageDir);
        _backupFilePath = Path.Combine(storageDir, "respaldo_base_de_datos.json");
    }

    public async Task<List<Person>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var list = await context.People
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .ToListAsync();

        foreach (var p in list)
        {
            SyncPersonImagesFromMemory(p);
        }

        return list;
    }

    public async Task<List<Person>> SearchPeopleAsync(string? searchTerm)
    {
        var list = await GetAllAsync();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return list;
        }

        // Normalizamos los términos de búsqueda para garantizar compatibilidad case-insensitive y acentos
        var rawTerm = searchTerm.Trim();
        var normalizedTerm = RemoveDiacritics(rawTerm).ToLowerInvariant();
        var tokens = normalizedTerm.Split(' ', StringSplitOptions.RemoveEmptyEntries);

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
        var person = await context.People.FirstOrDefaultAsync(p => p.Id == id);
        if (person != null)
        {
            SyncPersonImagesFromMemory(person);
        }
        return person;
    }

    public async Task<Person?> AuthenticateParticipantAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var rawUser = username.Trim();
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

        bool passwordValid =
            string.Equals(candidate.Password, rawPass, StringComparison.Ordinal) ||
            string.Equals(candidate.Password, rawPass, StringComparison.OrdinalIgnoreCase) ||
            Person.NormalizeIdentifier(candidate.Password) == normPass;

        if (!passwordValid)
            return null;

        SyncPersonImagesFromMemory(candidate);
        return candidate;
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

        if (string.IsNullOrWhiteSpace(person.Likes))
        {
            person.Likes = "Pendiente por definir en el portal";
        }
        if (string.IsNullOrWhiteSpace(person.Dislikes))
        {
            person.Dislikes = "Pendiente por definir en el portal";
        }

        SyncPersonImagesFromMemory(person);

        person.CreatedAt = DateTime.UtcNow;
        person.UpdatedAt = DateTime.UtcNow;
        context.People.Add(person);
        await context.SaveChangesAsync();
        await FlushAndBackupDatabaseAsync(context);
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
        existing.Likes = string.IsNullOrWhiteSpace(person.Likes) ? "Pendiente por definir en el portal" : person.Likes.Trim();
        existing.Dislikes = string.IsNullOrWhiteSpace(person.Dislikes) ? "Pendiente por definir en el portal" : person.Dislikes.Trim();
        existing.Allergies = person.Allergies?.Trim() ?? string.Empty;
        existing.Notes = person.Notes?.Trim();
        existing.UpdatedAt = DateTime.UtcNow;

        SyncPersonImagesFromMemory(existing);

        await context.SaveChangesAsync();
        await FlushAndBackupDatabaseAsync(context);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var person = await context.People.FirstOrDefaultAsync(p => p.Id == id);
        if (person == null) return false;

        await _imageStorage.RemoveProfilePhotoAsync(person.FirstName, person.LastName);
        await _imageStorage.RemoveGiftImageAsync(person.FirstName, person.LastName);

        context.People.Remove(person);
        await context.SaveChangesAsync();
        await FlushAndBackupDatabaseAsync(context);
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
        await DbInitializer.InitializeAsync(context, _imageStorage, _backupFilePath, forceReload: true);
        await FlushAndBackupDatabaseAsync(context);
    }

    private void SyncPersonImagesFromMemory(Person p)
    {
        var memProfileUrl = _imageStorage.GetProfilePhotoUrlIfExists(p.FirstName, p.LastName);
        if (!string.IsNullOrWhiteSpace(memProfileUrl))
        {
            p.ProfilePhotoPath = memProfileUrl;
        }

        var memGiftUrl = _imageStorage.GetGiftImageUrlIfExists(p.FirstName, p.LastName);
        if (!string.IsNullOrWhiteSpace(memGiftUrl))
        {
            p.GiftImagePath = memGiftUrl;
        }
    }

    private async Task FlushAndBackupDatabaseAsync(AppDbContext context)
    {
        try
        {
            // Forzar volcado inmediato a amigosecreto.db en disco
            await context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);");
        }
        catch { }

        try
        {
            var allPeople = await context.People.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
            foreach (var p in allPeople)
            {
                SyncPersonImagesFromMemory(p);
            }
            var json = JsonSerializer.Serialize(allPeople, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_backupFilePath, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AmigoSecreto] Error al guardar respaldo en archivo: {ex.Message}");
        }
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
