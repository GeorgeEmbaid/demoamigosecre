namespace AmigoSecreto.Services;

public class PersonImageMemoryEntry
{
    public string PersonKey { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    public string? ProfilePhotoContentType { get; set; }
    public string? ProfilePhotoBase64 { get; set; }
    public byte[]? ProfilePhotoBytes { get; set; }

    public string? GiftImageContentType { get; set; }
    public string? GiftImageBase64 { get; set; }
    public byte[]? GiftImageBytes { get; set; }

    public long VersionTicks { get; set; } = DateTime.UtcNow.Ticks;
}

public interface IImageStorageService
{
    /// <summary>
    /// Carga todos los archivos .txt de la carpeta ImagenesParticipantes en memoria al iniciar el servidor
    /// </summary>
    void LoadAllFromDiskIntoMemory();

    /// <summary>
    /// Guarda o modifica la foto de perfil en memoria y en el archivo {Nombre}_{Apellido}.txt
    /// </summary>
    Task<string> SaveProfilePhotoAsync(string firstName, string lastName, byte[] imageBytes, string contentType);

    /// <summary>
    /// Guarda o modifica la imagen del regalo deseado en memoria y en el archivo {Nombre}_{Apellido}.txt
    /// </summary>
    Task<string> SaveGiftImageAsync(string firstName, string lastName, byte[] imageBytes, string contentType);

    /// <summary>
    /// Elimina la foto de perfil en memoria y actualiza el archivo {Nombre}_{Apellido}.txt
    /// </summary>
    Task RemoveProfilePhotoAsync(string firstName, string lastName);

    /// <summary>
    /// Elimina la imagen del regalo en memoria y actualiza el archivo {Nombre}_{Apellido}.txt
    /// </summary>
    Task RemoveGiftImageAsync(string firstName, string lastName);

    /// <summary>
    /// Obtiene los bytes de la foto de perfil directamente desde la memoria
    /// </summary>
    (byte[]? Bytes, string ContentType) GetProfilePhotoFromMemory(string personKey);

    /// <summary>
    /// Obtiene los bytes de la imagen del regalo directamente desde la memoria
    /// </summary>
    (byte[]? Bytes, string ContentType) GetGiftImageFromMemory(string personKey);

    /// <summary>
    /// Devuelve la URL en memoria si la persona tiene foto de perfil cargada en su .txt
    /// </summary>
    string? GetProfilePhotoUrlIfExists(string firstName, string lastName);

    /// <summary>
    /// Devuelve la URL en memoria si la persona tiene imagen de regalo cargada en su .txt
    /// </summary>
    string? GetGiftImageUrlIfExists(string firstName, string lastName);

    /// <summary>
    /// Genera el nombre clave normalizado {Nombre}_{Apellido} para el archivo .txt
    /// </summary>
    string GetPersonKey(string firstName, string lastName);
}
