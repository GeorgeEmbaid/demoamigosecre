using AmigoSecreto.Models;
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

    public static async Task InitializeAsync(AppDbContext context, bool forceReload = false)
    {
        await context.Database.EnsureCreatedAsync();

        // Verificar si el esquema antiguo carece de las nuevas columnas (Username, Password, ProfilePhotoPath, etc.)
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
            // Asegurarnos de que todos los registros existentes tengan Username, Password y dominio @bncenlinea.net
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
                if (string.IsNullOrWhiteSpace(p.Email) || p.Email.EndsWith("@bnc.com.ve", StringComparison.OrdinalIgnoreCase))
                {
                    p.Email = Person.GenerateDefaultEmail(p.FirstName, p.LastName);
                    updatedAny = true;
                }
            }
            if (updatedAny)
            {
                await context.SaveChangesAsync();
                Console.WriteLine("[AmigoSecreto] Updated existing records with @bncenlinea.net and credentials.");
            }
            return;
        }

        Console.WriteLine("[AmigoSecreto] Seeding official BNC participants with @bncenlinea.net and login credentials...");

        // Lista oficial de 51 participantes BNC provista por el usuario
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
                Likes = "Pendiente por definir en el portal",
                Dislikes = "Pendiente por definir en el portal",
                Allergies = "",
                Notes = "Participante oficial del Amigo Secreto BNC 2026."
            });

            colorIdx++;
        }

        await context.People.AddRangeAsync(people);
        await context.SaveChangesAsync();
        Console.WriteLine($"[AmigoSecreto] Successfully seeded {people.Count} participants with @bncenlinea.net!");
    }
}
