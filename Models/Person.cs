using System.ComponentModel.DataAnnotations;

namespace AmigoSecreto.Models;

public class Person
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(60, ErrorMessage = "El nombre no puede exceder 60 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(60, ErrorMessage = "El apellido no puede exceder 60 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres")]
    public string? Email { get; set; }

    [StringLength(80)]
    public string? Department { get; set; }

    [StringLength(30)]
    public string? ShirtSize { get; set; }

    [StringLength(30)]
    public string? ShoeSize { get; set; }

    [StringLength(40)]
    public string? FavoriteColor { get; set; }

    [StringLength(20)]
    public string? AvatarColor { get; set; } = "#002855";

    /// <summary>
    /// Lista de gustos separados por saltos de línea o comas
    /// </summary>
    public string Likes { get; set; } = string.Empty;

    /// <summary>
    /// Lista de cosas que NO le gustan o prefiere evitar
    /// </summary>
    public string Dislikes { get; set; } = string.Empty;

    /// <summary>
    /// Alergias, intolerancias o restricciones médicas/alimenticias
    /// </summary>
    public string Allergies { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Propiedades de conveniencia calculadas
    public string FullName => $"{FirstName} {LastName}".Trim();

    public string Initials
    {
        get
        {
            var first = !string.IsNullOrWhiteSpace(FirstName) ? FirstName.Trim()[0].ToString() : "";
            var last = !string.IsNullOrWhiteSpace(LastName) ? LastName.Trim()[0].ToString() : "";
            return $"{first}{last}".ToUpper();
        }
    }

    public bool HasCustomLikes => 
        !string.IsNullOrWhiteSpace(Likes) && 
        !Likes.Contains("Pendiente por definir", StringComparison.OrdinalIgnoreCase);

    public bool HasCustomDislikes => 
        !string.IsNullOrWhiteSpace(Dislikes) && 
        !Dislikes.Contains("Pendiente por definir", StringComparison.OrdinalIgnoreCase);

    public List<string> LikesList => ParseList(Likes);
    public List<string> DislikesList => ParseList(Dislikes);
    public List<string> AllergiesList => ParseList(Allergies);

    private static List<string> ParseList(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return new List<string>();

        return raw.Split(new[] { '\r', '\n', ';', '•', '-' }, StringSplitOptions.RemoveEmptyEntries)
                  .Select(x => x.Trim())
                  .Where(x => !string.IsNullOrWhiteSpace(x))
                  .ToList();
    }
}
