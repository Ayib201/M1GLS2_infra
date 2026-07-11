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

    // Les 3 classes métier de l'application (en plus de Utilisateur) :
    // Projet -> Tache -> Commentaire, chacune rattachée à la précédente.
    public DbSet<Projet> Projets => Set<Projet>();
    public DbSet<Tache> Taches => Set<Tache>();
    public DbSet<Commentaire> Commentaires => Set<Commentaire>();

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

        modelBuilder.Entity<Projet>(entity =>
        {
            entity.ToTable("projets");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Nom).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);

            // Un utilisateur peut avoir plusieurs projets ; si son profil est
            // supprimé, ses projets le sont aussi en cascade (évite des
            // projets "orphelins" sans propriétaire).
            entity.HasOne(p => p.Utilisateur)
                  .WithMany()
                  .HasForeignKey(p => p.UtilisateurId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.UtilisateurId); // accélère "mes projets" (filtre très fréquent)
        });

        modelBuilder.Entity<Tache>(entity =>
        {
            entity.ToTable("taches");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Titre).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Description).HasMaxLength(2000);
            // Stocke l'enum sous forme de texte lisible ("AFaire", "EnCours"...)
            // plutôt qu'un entier opaque -- plus facile à lire/déboguer directement en base.
            entity.Property(t => t.Statut).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(t => t.Projet)
                  .WithMany(p => p.Taches)
                  .HasForeignKey(t => t.ProjetId)
                  .OnDelete(DeleteBehavior.Cascade); // supprimer un projet supprime ses tâches

            entity.HasIndex(t => t.ProjetId);
        });

        modelBuilder.Entity<Commentaire>(entity =>
        {
            entity.ToTable("commentaires");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Contenu).IsRequired().HasMaxLength(2000);

            entity.HasOne(c => c.Tache)
                  .WithMany(t => t.Commentaires)
                  .HasForeignKey(c => c.TacheId)
                  .OnDelete(DeleteBehavior.Cascade); // supprimer une tâche supprime ses commentaires

            // Restrict (et non Cascade) : supprimer un utilisateur ne doit
            // jamais supprimer silencieusement les commentaires qu'il a écrits
            // ailleurs -- comportement volontairement différent de Projet/Tache
            // pour illustrer que "cascade partout" n'est pas toujours le bon choix.
            entity.HasOne(c => c.Utilisateur)
                  .WithMany()
                  .HasForeignKey(c => c.UtilisateurId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.TacheId);
        });
    }
}
