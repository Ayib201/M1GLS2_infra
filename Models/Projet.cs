namespace M1GLS2_infra.Models;

/// <summary>
/// Un projet appartient à un seul <see cref="Utilisateur"/> (son propriétaire)
/// et regroupe plusieurs <see cref="Tache"/>. C'est la racine de la hiérarchie
/// métier de cette application : Utilisateur → Projet → Tache → Commentaire.
/// </summary>
public class Projet
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Nom { get; set; }

    public string? Description { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Clé étrangère explicite vers le propriétaire. On la garde ici (et pas
    // seulement via la propriété de navigation ci-dessous) pour pouvoir
    // filtrer les requêtes EF Core directement sur cette colonne
    // (ex: Where(p => p.UtilisateurId == idConnecte)), sans avoir à charger
    // l'utilisateur entier à chaque fois.
    public Guid UtilisateurId { get; set; }

    public Utilisateur? Utilisateur { get; set; }

    // Collection de navigation : permet à EF Core de charger les tâches
    // liées à ce projet (Include(p => p.Taches)) quand on en a besoin.
    public ICollection<Tache> Taches { get; set; } = new List<Tache>();
}
