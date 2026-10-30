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
            // Bloque 1 (16 participantes)
            new { First = "Sarahy", Last = "Barragán", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Wilmary", Last = "Maduro", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Leonardo", Last = "Chacón", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Alix", Last = "Puello", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Keynner", Last = "Aguilar", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Carlos", Last = "Nuñez", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Sorianyely", Last = "Morin", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Pablo", Last = "Barboza", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Jesús", Last = "Gomes", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "George", Last = "Embaid", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Kevin", Last = "Solorzano", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Erick", Last = "Adames", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Julián", Last = "Esteves", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Susana", Last = "Escobar", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Jhon", Last = "Erice", Dept = "Tecnología e Infraestructura BNC" },
            new { First = "Jesús", Last = "Noguera", Dept = "Tecnología e Infraestructura BNC" },

            // Bloque 2 (16 participantes)
            new { First = "José", Last = "Sanabria", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Katherine", Last = "Mendible", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Darwin", Last = "García", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Gabriel", Last = "Tagliafico", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Kervin", Last = "Azuaje", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Albert", Last = "Rangel", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Rosbel", Last = "Rodríguez", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Miguel", Last = "Suarez", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Edinson", Last = "Cabello", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Jesús", Last = "Colina", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Yldemaro", Last = "Romero", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Junior", Last = "Romero", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Rafael", Last = "Lozano", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Andrés", Last = "Parra", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Yhusleika", Last = "Molina", Dept = "Desarrollo y Canales Digitales BNC" },
            new { First = "Victor", Last = "Obelmejía", Dept = "Desarrollo y Canales Digitales BNC" },

            // Bloque 3 (19 participantes)
            new { First = "Enrique", Last = "Sanchez", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Jonathan", Last = "Colucci", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Michael", Last = "Bastidas", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Lileska", Last = "Romero", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Jose Manuel", Last = "Quintal", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Jose Luis", Last = "Reyes", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Greadary", Last = "Garcia", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Sheyla", Last = "Tovar", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Abraham", Last = "Figueroa", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Karilyn", Last = "Carvajal", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Nestor", Last = "Villegas", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Gustavo", Last = "Suarez", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Eduardo", Last = "Lobo", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Pablo", Last = "Romero", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Yessika", Last = "Mata", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Alexander", Last = "Diaz", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Zenon", Last = "Avariano", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Tokio", Last = "Franco", Dept = "Operaciones y Sistemas BNC" },
            new { First = "Luis", Last = "Rendon", Dept = "Operaciones y Sistemas BNC" }
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
                Department = item.Dept,
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
