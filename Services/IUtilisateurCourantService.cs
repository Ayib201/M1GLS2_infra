using System.Security.Claims;
using M1GLS2_infra.Models;

namespace M1GLS2_infra.Services;

/// <summary>
/// Résout l'<see cref="Utilisateur"/> correspondant au jeton JWT présenté,
/// en le créant automatiquement en base au tout premier appel ("auto-
/// provisioning"). Remplace l'ancien bouton explicite "Créer mon profil" :
/// dès qu'un utilisateur connecté crée son premier projet, son profil existe
/// déjà. Utilisé par IProjetService, ITacheService et ICommentaireService
/// pour savoir "qui fait cette requête ?" avant de vérifier les droits.
/// </summary>
public interface IUtilisateurCourantService
{
    Task<Utilisateur> ObtenirOuCreerAsync(ClaimsPrincipal utilisateurConnecte);
}
