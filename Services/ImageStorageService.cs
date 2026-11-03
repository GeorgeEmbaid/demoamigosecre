using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

namespace AmigoSecreto.Services;

public class ImageStorageService : IImageStorageService
{
    private readonly string _storageFolder;
    private readonly ConcurrentDictionary<string, PersonImageMemoryEntry> _memoryCache = new(StringComparer.OrdinalIgnoreCase);

    public ImageStorageService(IWebHostEnvironment env)
    {
        _storageFolder = Path.Combine(env.ContentRootPath, "ImagenesParticipantes");
        Directory.CreateDirectory(_storageFolder);
        LoadAllFromDiskIntoMemory();
    }

    public void LoadAllFromDiskIntoMemory()
    {
        try
        {
            if (!Directory.Exists(_storageFolder))
            {
                Directory.CreateDirectory(_storageFolder);
                return;
            }

            var txtFiles = Directory.GetFiles(_storageFolder, "*.txt");
            foreach (var filePath in txtFiles)
            {
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrWhiteSpace(fileNameWithoutExt)) continue;

                var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                var entry = new PersonImageMemoryEntry
                {
                    PersonKey = fileNameWithoutExt,
                    FullName = fileNameWithoutExt.Replace('_', ' '),
                    VersionTicks = File.GetLastWriteTimeUtc(filePath).Ticks
                };

                foreach (var line in lines)
                {
                    var idx = line.IndexOf('=');
                    if (idx <= 0) continue;

                    var key = line[..idx].Trim();
                    var val = line[(idx + 1)..].Trim();

                    switch (key)
                    {
                        case "PARTICIPANTE":
                            entry.FullName = val;
                            break;
                        case "PERFIL_TYPE":
                            entry.ProfilePhotoContentType = string.IsNullOrWhiteSpace(val) ? "image/jpeg" : val;
                            break;
                        case "PERFIL_BASE64":
                            if (!string.IsNullOrWhiteSpace(val))
                            {
                                entry.ProfilePhotoBase64 = val;
                                try { entry.ProfilePhotoBytes = Convert.FromBase64String(val); } catch { }
                            }
                            break;
                        case "REGALO_TYPE":
                            entry.GiftImageContentType = string.IsNullOrWhiteSpace(val) ? "image/jpeg" : val;
                            break;
                        case "REGALO_BASE64":
                            if (!string.IsNullOrWhiteSpace(val))
                            {
                                entry.GiftImageBase64 = val;
                                try { entry.GiftImageBytes = Convert.FromBase64String(val); } catch { }
                            }
                            break;
                    }
                }

                _memoryCache[fileNameWithoutExt] = entry;
            }

            Console.WriteLine($"[AmigoSecreto] Cargados {_memoryCache.Count} archivos .txt de imágenes en memoria desde '{_storageFolder}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AmigoSecreto] Error al cargar imágenes .txt en memoria: {ex.Message}");
        }
    }

    public async Task<string> SaveProfilePhotoAsync(string firstName, string lastName, byte[] imageBytes, string contentType)
    {
        var personKey = GetPersonKey(firstName, lastName);
        var fullName = $"{firstName} {lastName}".Trim();
        var base64 = Convert.ToBase64String(imageBytes);
        var safeContentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType;

        var entry = _memoryCache.GetOrAdd(personKey, _ => new PersonImageMemoryEntry
        {
            PersonKey = personKey,
            FullName = fullName
        });

        entry.FullName = fullName;
        entry.ProfilePhotoContentType = safeContentType;
        entry.ProfilePhotoBase64 = base64;
        entry.ProfilePhotoBytes = imageBytes;
        entry.VersionTicks = DateTime.UtcNow.Ticks;

        await WritePersonTxtFileAsync(entry);
        return $"imagenes-memoria/{personKey}/perfil?v={entry.VersionTicks}";
    }

    public async Task<string> SaveGiftImageAsync(string firstName, string lastName, byte[] imageBytes, string contentType)
    {
        var personKey = GetPersonKey(firstName, lastName);
        var fullName = $"{firstName} {lastName}".Trim();
        var base64 = Convert.ToBase64String(imageBytes);
        var safeContentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType;

        var entry = _memoryCache.GetOrAdd(personKey, _ => new PersonImageMemoryEntry
        {
            PersonKey = personKey,
            FullName = fullName
        });

        entry.FullName = fullName;
        entry.GiftImageContentType = safeContentType;
        entry.GiftImageBase64 = base64;
        entry.GiftImageBytes = imageBytes;
        entry.VersionTicks = DateTime.UtcNow.Ticks;

        await WritePersonTxtFileAsync(entry);
        return $"imagenes-memoria/{personKey}/regalo?v={entry.VersionTicks}";
    }

    public async Task RemoveProfilePhotoAsync(string firstName, string lastName)
    {
        var personKey = GetPersonKey(firstName, lastName);
        if (_memoryCache.TryGetValue(personKey, out var entry))
        {
            entry.ProfilePhotoBase64 = null;
            entry.ProfilePhotoBytes = null;
            entry.ProfilePhotoContentType = null;
            entry.VersionTicks = DateTime.UtcNow.Ticks;
            await WritePersonTxtFileAsync(entry);
        }
    }

    public async Task RemoveGiftImageAsync(string firstName, string lastName)
    {
        var personKey = GetPersonKey(firstName, lastName);
        if (_memoryCache.TryGetValue(personKey, out var entry))
        {
            entry.GiftImageBase64 = null;
            entry.GiftImageBytes = null;
            entry.GiftImageContentType = null;
            entry.VersionTicks = DateTime.UtcNow.Ticks;
            await WritePersonTxtFileAsync(entry);
        }
    }

    public (byte[]? Bytes, string ContentType) GetProfilePhotoFromMemory(string personKey)
    {
        if (_memoryCache.TryGetValue(personKey, out var entry) && entry.ProfilePhotoBytes is { Length: > 0 })
        {
            return (entry.ProfilePhotoBytes, entry.ProfilePhotoContentType ?? "image/jpeg");
        }
        return (null, "image/jpeg");
    }

    public (byte[]? Bytes, string ContentType) GetGiftImageFromMemory(string personKey)
    {
        if (_memoryCache.TryGetValue(personKey, out var entry) && entry.GiftImageBytes is { Length: > 0 })
        {
            return (entry.GiftImageBytes, entry.GiftImageContentType ?? "image/jpeg");
        }
        return (null, "image/jpeg");
    }

    public string? GetProfilePhotoUrlIfExists(string firstName, string lastName)
    {
        var personKey = GetPersonKey(firstName, lastName);
        if (_memoryCache.TryGetValue(personKey, out var entry) && entry.ProfilePhotoBytes is { Length: > 0 })
        {
            return $"imagenes-memoria/{personKey}/perfil?v={entry.VersionTicks}";
        }
        return null;
    }

    public string? GetGiftImageUrlIfExists(string firstName, string lastName)
    {
        var personKey = GetPersonKey(firstName, lastName);
        if (_memoryCache.TryGetValue(personKey, out var entry) && entry.GiftImageBytes is { Length: > 0 })
        {
            return $"imagenes-memoria/{personKey}/regalo?v={entry.VersionTicks}";
        }
        return null;
    }

    public string GetPersonKey(string firstName, string lastName)
    {
        var raw = $"{firstName?.Trim()} {lastName?.Trim()}".Trim();
        if (string.IsNullOrWhiteSpace(raw)) return "Participante";

        var normalized = raw.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        var lastWasUnderscore = false;

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark) continue;

            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
                lastWasUnderscore = false;
            }
            else if (char.IsWhiteSpace(c) || c == '_' || c == '-')
            {
                if (!lastWasUnderscore && sb.Length > 0)
                {
                    sb.Append('_');
                    lastWasUnderscore = true;
                }
            }
        }

        return sb.ToString().Trim('_');
    }

    private async Task WritePersonTxtFileAsync(PersonImageMemoryEntry entry)
    {
        Directory.CreateDirectory(_storageFolder);
        var filePath = Path.Combine(_storageFolder, $"{entry.PersonKey}.txt");

        // Si se eliminaron ambas imágenes, borrar el archivo .txt si existía
        if (string.IsNullOrWhiteSpace(entry.ProfilePhotoBase64) && string.IsNullOrWhiteSpace(entry.GiftImageBase64))
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"PARTICIPANTE={entry.FullName}");
        sb.AppendLine($"UPDATED_AT={DateTime.UtcNow:O}");
        sb.AppendLine($"PERFIL_TYPE={entry.ProfilePhotoContentType ?? ""}");
        sb.AppendLine($"PERFIL_BASE64={entry.ProfilePhotoBase64 ?? ""}");
        sb.AppendLine($"REGALO_TYPE={entry.GiftImageContentType ?? ""}");
        sb.AppendLine($"REGALO_BASE64={entry.GiftImageBase64 ?? ""}");

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }
}
