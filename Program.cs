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

// Connection string with absolute path to ContentRootPath
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "amigosecreto.db");
var connectionString = $"Data Source={dbPath}";
Console.WriteLine($"[AmigoSecreto] Database path: {dbPath}");

// Add EF Core DbContextFactory for SQLite
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Register domain services
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Initialize and seed SQLite database
try
{
    Console.WriteLine("[AmigoSecreto] Initializing database...");
    using (var scope = app.Services.CreateScope())
    {
        var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var dbContext = await contextFactory.CreateDbContextAsync();
        await DbInitializer.InitializeAsync(dbContext);
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
