using M1GLS2_infra.Models;

namespace M1GLS2_infra.Services;

// Records = types immuables, comparés par valeur -- parfaits pour des DTOs
// (Data Transfer Objects) qui ne font que transporter des données entre le
// client et le service, sans logique ni identité propre.

public sealed record CreerProjetRequest(string Nom, string? Description);

public sealed record CreerTacheRequest(string Titre, string? Description, DateTime? DateEcheance);

public sealed record MettreAJourTacheRequest(
    string Titre,
    string? Description,
    StatutTache Statut,
    DateTime? DateEcheance);

public sealed record CreerCommentaireRequest(string Contenu);

/// <summary>
/// "ProvientDuCache" permet au contrôleur d'exposer un header HTTP
/// (X-Cache-Status: HIT/MISS) sans que ProjetService ait besoin de
/// connaître quoi que ce soit sur HTTP -- le service reste concentré sur
/// la logique métier, le contrôleur reste seul responsable de la traduction
/// HTTP (voir Controllers/ProjetsController.cs).
/// </summary>
public sealed record ResultatListeProjets(IReadOnlyList<Projet> Projets, bool ProvientDuCache);

/// <summary>Même rôle que ResultatListeProjets, pour la liste des tâches d'un projet.</summary>
public sealed record ResultatListeTaches(IReadOnlyList<Tache> Taches, bool ProvientDuCache);

/// <summary>Même rôle que ResultatListeProjets, pour la liste des commentaires d'une tâche.</summary>
public sealed record ResultatListeCommentaires(IReadOnlyList<Commentaire> Commentaires, bool ProvientDuCache);
