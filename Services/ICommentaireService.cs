using System.Security.Claims;
using M1GLS2_infra.Models;

namespace M1GLS2_infra.Services;

/// <summary>
/// Logique métier des commentaires, toujours accédés via leur tâche et leur
/// projet parents (route imbriquée
/// /api/v1/projets/{projetId}/taches/{tacheId}/commentaires).
/// </summary>
public interface ICommentaireService
{
    Task<ResultatListeCommentaires?> ListerCommentairesAsync(
        Guid projetId, Guid tacheId, ClaimsPrincipal utilisateurConnecte);

    Task<Commentaire?> CreerCommentaireAsync(
        Guid projetId, Guid tacheId, CreerCommentaireRequest requete, ClaimsPrincipal utilisateurConnecte);

    Task<bool> SupprimerCommentaireAsync(
        Guid projetId, Guid tacheId, Guid commentaireId, ClaimsPrincipal utilisateurConnecte);
}
