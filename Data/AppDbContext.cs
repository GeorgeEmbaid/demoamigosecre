using Microsoft.EntityFrameworkCore;
using AmigoSecreto.Models;

namespace AmigoSecreto.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Person> People => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(60);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(60);
            entity.Property(e => e.Username).HasMaxLength(60);
            entity.Property(e => e.Password).HasMaxLength(100);

            // Indexes for faster search and authentication
            entity.HasIndex(e => e.FirstName);
            entity.HasIndex(e => e.LastName);
            entity.HasIndex(e => e.Username);
        });
    }
}
