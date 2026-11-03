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

        if (forceReload)
        {
            context.People.RemoveRange(context.People);
            await context.SaveChangesAsync();
        }
        else if (await context.People.AnyAsync())
        {
            Console.WriteLine("[AmigoSecreto] Database already contains records.");
            return;
        }

        Console.WriteLine("[AmigoSecreto] Seeding official BNC participants...");

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
            var emailUser = $"{item.First.ToLowerInvariant().Replace(" ", ".")}.{item.Last.ToLowerInvariant().Replace(" ", ".")}"
                .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");

            people.Add(new Person
            {
                FirstName = item.First,
                LastName = item.Last,
                Department = null,
                Email = $"{emailUser}@bnc.com.ve",
                AvatarColor = AvatarColors[colorIdx % AvatarColors.Length],
                Likes = "Pendiente por definir en el portal administrativo",
                Dislikes = "Pendiente por definir en el portal administrativo",
                Allergies = "",
                Notes = "Participante oficial del Amigo Secreto BNC 2026."
            });

            colorIdx++;
        }

        await context.People.AddRangeAsync(people);
        await context.SaveChangesAsync();
        Console.WriteLine($"[AmigoSecreto] Successfully seeded {people.Count} participants!");
    }
}
