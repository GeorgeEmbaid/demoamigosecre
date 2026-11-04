using System.Text;
using System.Text.Json;
using AmigoSecreto.Models;
using AmigoSecreto.Services;
using Microsoft.EntityFrameworkCore;

namespace AmigoSecreto.Data;

public static class DbInitializer
{
    private static readonly string[] AvatarColors = new[]
    {
        "#002855", // BNC Navy
        "#FF6600", // BNC Orange
        "#059669", // Emerald
        "#0D9488", // Deep Teal
        "#7C3AED", // Purple
        "#001933", // Dark Navy
        "#475569", // Slate
        "#DC2626"  // Crimson
    };

    public static async Task InitializeAsync(
        AppDbContext context,
        IImageStorageService? imageStorage = null,
        string? backupFilePath = null,
        bool forceReload = false)
    {
        await context.Database.EnsureCreatedAsync();

        try
        {
            await context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = FULL;");
        }
        catch { }

        // Verificar si el esquema antiguo carece de las nuevas columnas
        try
        {
            _ = await context.People.Select(p => new { p.Username, p.Password, p.ProfilePhotoPath, p.GiftImagePath, p.GiftReferenceUrl }).FirstOrDefaultAsync();
        }
        catch
        {
            Console.WriteLine("[AmigoSecreto] Recreating database schema to include authentication & image fields...");
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            forceReload = true;
        }

        if (forceReload)
        {
            context.People.RemoveRange(context.People);
            await context.SaveChangesAsync();
        }
        else if (await context.People.AnyAsync())
        {
            // Asegurarnos de que todos los registros existentes tengan Username, Password e imágenes desde memoria (.txt)
            var existingPeople = await context.People.ToListAsync();
            bool updatedAny = false;
            foreach (var p in existingPeople)
            {
                if (string.IsNullOrWhiteSpace(p.Username))
                {
                    p.Username = Person.GenerateDefaultUsername(p.FirstName, p.LastName);
                    updatedAny = true;
                }
                if (string.IsNullOrWhiteSpace(p.Password))
                {
                    p.Password = Person.GenerateDefaultPassword(p.LastName);
                    updatedAny = true;
                }
                if (string.IsNullOrWhiteSpace(p.Email) || p.Email.Contains("bnc", StringComparison.OrdinalIgnoreCase))
                {
                    p.Email = Person.GenerateDefaultEmail(p.FirstName, p.LastName);
                    updatedAny = true;
                }
                if (p.Notes != null && p.Notes.Contains("BNC", StringComparison.OrdinalIgnoreCase))
                {
                    p.Notes = p.Notes.Replace(" BNC", "").Replace("BNC ", "").Replace("BNC", "").Trim();
                    updatedAny = true;
                }
                if (imageStorage != null)
                {
                    var memProfile = imageStorage.GetProfilePhotoUrlIfExists(p.FirstName, p.LastName);
                    if (!string.IsNullOrWhiteSpace(memProfile) && p.ProfilePhotoPath != memProfile)
                    {
                        p.ProfilePhotoPath = memProfile;
                        updatedAny = true;
                    }
                    var memGift = imageStorage.GetGiftImageUrlIfExists(p.FirstName, p.LastName);
                    if (!string.IsNullOrWhiteSpace(memGift) && p.GiftImagePath != memGift)
                    {
                        p.GiftImagePath = memGift;
                        updatedAny = true;
                    }
                }
            }
            if (updatedAny)
            {
                await context.SaveChangesAsync();
                try { await context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);"); } catch { }
            }
            return;
        }

        // Si la base de datos SQLite está vacía y existe el archivo de respaldo en disco, restaurar desde el respaldo
        if (!forceReload && !string.IsNullOrWhiteSpace(backupFilePath) && File.Exists(backupFilePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(backupFilePath, Encoding.UTF8);
                var backupPeople = JsonSerializer.Deserialize<List<Person>>(json);
                if (backupPeople is { Count: > 0 })
                {
                    foreach (var bp in backupPeople)
                    {
                        bp.Id = 0; // Permitir inserción limpia
                        if (imageStorage != null)
                        {
                            bp.ProfilePhotoPath = imageStorage.GetProfilePhotoUrlIfExists(bp.FirstName, bp.LastName) ?? bp.ProfilePhotoPath;
                            bp.GiftImagePath = imageStorage.GetGiftImageUrlIfExists(bp.FirstName, bp.LastName) ?? bp.GiftImagePath;
                        }
                    }
                    await context.People.AddRangeAsync(backupPeople);
                    await context.SaveChangesAsync();
                    try { await context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);"); } catch { }
                    Console.WriteLine($"[AmigoSecreto] Restaurados {backupPeople.Count} participantes desde archivo de respaldo.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AmigoSecreto] No se pudo restaurar desde archivo de respaldo: {ex.Message}");
            }
        }

        Console.WriteLine("[AmigoSecreto] Seeding official participants with login credentials...");

        var rawData = new[]
        {
            new { First = "Sarahy", Last = "Barragán" },
            new { First = "Wilmary", Last = "Maduro" },
            new { First = "Leonardo", Last = "Chacón" },
            new { First = "Alix", Last = "Puello" },
            new { First = "Keynner", Last = "Aguilar" },
            new { First = "Carlos", Last = "Nuñez" },
            new { First = "Sorianyely", Last = "Morin" },
            new { First = "Pablo", Last = "Barboza" },
            new { First = "Jesús", Last = "Gomes" },
            new { First = "George", Last = "Embaid" },
            new { First = "Kevin", Last = "Solorzano" },
            new { First = "Erick", Last = "Adames" },
            new { First = "Julián", Last = "Esteves" },
            new { First = "Susana", Last = "Escobar" },
            new { First = "Jhon", Last = "Erice" },
            new { First = "Jesús", Last = "Noguera" },

            new { First = "José", Last = "Sanabria" },
            new { First = "Katherine", Last = "Mendible" },
            new { First = "Darwin", Last = "García" },
            new { First = "Gabriel", Last = "Tagliafico" },
            new { First = "Kervin", Last = "Azuaje" },
            new { First = "Albert", Last = "Rangel" },
            new { First = "Rosbel", Last = "Rodríguez" },
            new { First = "Miguel", Last = "Suarez" },
            new { First = "Edinson", Last = "Cabello" },
            new { First = "Jesús", Last = "Colina" },
            new { First = "Yldemaro", Last = "Romero" },
            new { First = "Junior", Last = "Romero" },
            new { First = "Rafael", Last = "Lozano" },
            new { First = "Andrés", Last = "Parra" },
            new { First = "Yhusleika", Last = "Molina" },
            new { First = "Victor", Last = "Obelmejía" },

            new { First = "Enrique", Last = "Sanchez" },
            new { First = "Jonathan", Last = "Colucci" },
            new { First = "Michael", Last = "Bastidas" },
            new { First = "Lileska", Last = "Romero" },
            new { First = "Jose Manuel", Last = "Quintal" },
            new { First = "Jose Luis", Last = "Reyes" },
            new { First = "Greadary", Last = "Garcia" },
            new { First = "Sheyla", Last = "Tovar" },
            new { First = "Abraham", Last = "Figueroa" },
            new { First = "Karilyn", Last = "Carvajal" },
            new { First = "Nestor", Last = "Villegas" },
            new { First = "Gustavo", Last = "Suarez" },
            new { First = "Eduardo", Last = "Lobo" },
            new { First = "Pablo", Last = "Romero" },
            new { First = "Yessika", Last = "Mata" },
            new { First = "Alexander", Last = "Diaz" },
            new { First = "Zenon", Last = "Avariano" },
            new { First = "Tokio", Last = "Franco" },
            new { First = "Luis", Last = "Rendon" }
        };

        var people = new List<Person>();
        int colorIdx = 0;

        foreach (var item in rawData)
        {
            var username = Person.GenerateDefaultUsername(item.First, item.Last);
            var password = Person.GenerateDefaultPassword(item.Last);
            var email = Person.GenerateDefaultEmail(item.First, item.Last);

            people.Add(new Person
            {
                FirstName = item.First,
                LastName = item.Last,
                Username = username,
                Password = password,
                Department = null,
                Email = email,
                AvatarColor = AvatarColors[colorIdx % AvatarColors.Length],
                ProfilePhotoPath = imageStorage?.GetProfilePhotoUrlIfExists(item.First, item.Last),
                GiftImagePath = imageStorage?.GetGiftImageUrlIfExists(item.First, item.Last),
                Likes = "Pendiente por definir en el portal",
                Dislikes = "Pendiente por definir en el portal",
                Allergies = "",
                Notes = "Participante oficial del Amigo Secreto 2026."
            });

            colorIdx++;
        }

        await context.People.AddRangeAsync(people);
        await context.SaveChangesAsync();
        try { await context.Database.ExecuteSqlRawAsync("PRAGMA wal_checkpoint(TRUNCATE);"); } catch { }
        Console.WriteLine($"[AmigoSecreto] Successfully seeded {people.Count} participants!");
    }
}
