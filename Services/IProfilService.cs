using System.Security.Claims;

namespace M1GLS2_infra.Services;

/// <summary>
/// Logique métier de création de profil : extraire l'identité depuis le
/// jeton JWT et l'enregistrer en base.
///
/// Pourquoi cette logique n'est-elle PAS directement dans le contrôleur ?
/// Principe de responsabilité unique (le "S" de SOLID) : un contrôleur ne
/// devrait "parler HTTP" (recevoir une requête, appeler la bonne logique,
/// renvoyer une réponse) et rien d'autre. En sortant la logique métier et
/// l'accès à la base dans ce service, le contrôleur reste court et lisible,
/// et cette logique devient testable indépendamment de tout contexte HTTP
/// (un test unitaire peut appeler CreerProfilDepuisJetonAsync directement,
/// sans monter un serveur web).
/// </summary>
public interface IProfilService
{
    Task<ProfilCreationResultat> CreerProfilDepuisJetonAsync(ClaimsPrincipal utilisateurConnecte);
}
