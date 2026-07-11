namespace M1GLS2_infra.Models;

/// <summary>
/// Une tâche appartient à un seul <see cref="Projet"/> et peut recevoir
/// plusieurs <see cref="Commentaire"/>.
/// </summary>
public class Tache
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Titre { get; set; }

    public string? Description { get; set; }

    public StatutTache Statut { get; set; } = StatutTache.AFaire;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public DateTime? DateEcheance { get; set; }

    public Guid ProjetId { get; set; }

    public Projet? Projet { get; set; }

    public ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();
}
