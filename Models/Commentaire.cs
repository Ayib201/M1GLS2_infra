namespace M1GLS2_infra.Models;

/// <summary>
/// Un commentaire appartient à une seule <see cref="Tache"/> et a été écrit
/// par un <see cref="Utilisateur"/> (l'auteur -- pas forcément le
/// propriétaire du projet, mais dans le cadre de cette démo, seul le
/// propriétaire a accès à ses projets, donc auteur == propriétaire).
/// </summary>
public class Commentaire
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Contenu { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public Guid TacheId { get; set; }

    public Tache? Tache { get; set; }

    public Guid UtilisateurId { get; set; }

    public Utilisateur? Utilisateur { get; set; }
}
