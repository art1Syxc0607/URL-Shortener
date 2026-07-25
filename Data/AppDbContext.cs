using Microsoft.EntityFrameworkCore;
using Data.Domain;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Url> Ulrs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Убедимся, что Pseudonym уникален и не может быть null
        modelBuilder.Entity<Url>()
            .HasIndex(u => u.Pseudonym)
            .IsUnique();
    }
}