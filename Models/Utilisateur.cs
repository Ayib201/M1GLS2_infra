namespace M1GLS2_infra.Models;

/// <summary>
/// Entité représentant un profil utilisateur stocké en base PostgreSQL.
///
/// C'est une classe "POCO" (Plain Old CLR Object) : une classe C# toute
/// simple, qui ne sait rien d'EF Core ni de PostgreSQL. C'est EF Core (dans
/// AppDbContext) qui se charge de la faire correspondre à une table SQL.
/// Cette séparation évite de mélanger "ce qu'est un utilisateur" (le métier)
/// avec "comment on le range en base" (la persistance).
/// </summary>
public class Utilisateur
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Nom { get; set; }

    public required string Email { get; set; }

    public DateTime DateInscription { get; set; } = DateTime.UtcNow;
}
