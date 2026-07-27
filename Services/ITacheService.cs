using System.Security.Claims;
using M1GLS2_infra.Models;

namespace M1GLS2_infra.Services;

/// <summary>
/// Logique métier des tâches, toujours accédées via leur projet parent
/// (route imbriquée /api/v1/projets/{projetId}/taches). Une tâche n'est
/// accessible que si le projet parent appartient à l'appelant.
/// </summary>
public interface ITacheService
{
    /// <summary>Retourne null si le projet parent n'existe pas ou n'appartient pas à l'appelant.</summary>
    Task<ResultatListeTaches?> ListerTachesAsync(Guid projetId, ClaimsPrincipal utilisateurConnecte);

    /// <summary>Retourne null si le projet parent n'existe pas ou n'appartient pas à l'appelant.</summary>
    Task<Tache?> CreerTacheAsync(Guid projetId, CreerTacheRequest requete, ClaimsPrincipal utilisateurConnecte);

    /// <summary>Retourne null si le projet ou la tâche n'existe pas / n'appartient pas à l'appelant.</summary>
    Task<Tache?> MettreAJourTacheAsync(
        Guid projetId, Guid tacheId, MettreAJourTacheRequest requete, ClaimsPrincipal utilisateurConnecte);

    Task<bool> SupprimerTacheAsync(Guid projetId, Guid tacheId, ClaimsPrincipal utilisateurConnecte);
}
