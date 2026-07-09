using M1GLS2_infra.Models;
using Microsoft.EntityFrameworkCore;

namespace M1GLS2_infra.Data;

/// <summary>
/// Point d'entrée EF Core vers la base PostgreSQL.
///
/// Un DbContext représente une "session de travail" avec la base : il suit
/// les objets chargés/modifiés en mémoire et sait générer le SQL correspondant
/// (SELECT/INSERT/UPDATE/DELETE) au moment de "SaveChangesAsync()".
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet<T> = "la table des Utilisateur" du point de vue du code C#.
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.ToTable("utilisateurs");   // nom de table en minuscules, convention PostgreSQL
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Nom).IsRequired().HasMaxLength(200);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(320);
            entity.HasIndex(u => u.Email).IsUnique();   // un email = un seul profil, imposé par la base elle-même
        });
    }
}
