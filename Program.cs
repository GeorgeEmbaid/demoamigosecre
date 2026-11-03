using AmigoSecreto.Components;
using AmigoSecreto.Data;
using AmigoSecreto.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure DataProtection to save keys inside workspace
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
Directory.CreateDirectory(keysFolder);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder));

// Connection string with absolute path to ContentRootPath (Pooling=False ensures immediate file write/flush)
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "amigosecreto.db");
var connectionString = $"Data Source={dbPath};Pooling=False";
Console.WriteLine($"[AmigoSecreto] Database path: {dbPath}");

// Add EF Core DbContextFactory for SQLite
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Register singleton service for .txt image storage + in-memory cache
builder.Services.AddSingleton<IImageStorageService, ImageStorageService>();

// Register domain services
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Initialize .txt images in memory and seed/sync SQLite database
try
{
    Console.WriteLine("[AmigoSecreto] Initializing database and loading .txt images into memory...");
    using (var scope = app.Services.CreateScope())
    {
        var imageStorage = scope.ServiceProvider.GetRequiredService<IImageStorageService>();
        imageStorage.LoadAllFromDiskIntoMemory();

        var backupFilePath = Path.Combine(builder.Environment.ContentRootPath, "ImagenesParticipantes", "respaldo_base_de_datos.json");
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        await DbInitializer.InitializeAsync(dbContext, imageStorage, backupFilePath);
    }
    Console.WriteLine("[AmigoSecreto] Database initialization completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"[AmigoSecreto] Database initialization error: {ex}");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

// Minimal API endpoints to serve images directly from the in-memory cache loaded from ImagenesParticipantes/{Nombre}_{Apellido}.txt
app.MapGet("/imagenes-memoria/{personKey}/perfil", (string personKey, IImageStorageService imageStorage) =>
{
    var (bytes, contentType) = imageStorage.GetProfilePhotoFromMemory(personKey);
    if (bytes is null || bytes.Length == 0)
    {
        return Results.NotFound();
    }
    return Results.File(bytes, contentType);
});

app.MapGet("/imagenes-memoria/{personKey}/regalo", (string personKey, IImageStorageService imageStorage) =>
{
    var (bytes, contentType) = imageStorage.GetGiftImageFromMemory(personKey);
    if (bytes is null || bytes.Length == 0)
    {
        return Results.NotFound();
    }
    return Results.File(bytes, contentType);
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
