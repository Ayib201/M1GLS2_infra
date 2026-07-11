using System.Security.Claims;
using M1GLS2_infra.Models;

namespace M1GLS2_infra.Services;

/// <summary>
/// Logique métier des projets. Racine de la hiérarchie : un projet
/// appartient à un seul utilisateur, qui est le seul à pouvoir le consulter,
/// le modifier ou le supprimer (isolation stricte entre utilisateurs).
/// </summary>
public interface IProjetService
{
    Task<IReadOnlyList<Projet>> ListerMesProjetsAsync(ClaimsPrincipal utilisateurConnecte);

    Task<Projet> CreerProjetAsync(CreerProjetRequest requete, ClaimsPrincipal utilisateurConnecte);

    /// <summary>Retourne null si le projet n'existe pas OU n'appartient pas à l'appelant.</summary>
    Task<Projet?> ObtenirProjetAsync(Guid projetId, ClaimsPrincipal utilisateurConnecte);

    /// <summary>Retourne false si le projet n'existe pas ou n'appartient pas à l'appelant.</summary>
    Task<bool> SupprimerProjetAsync(Guid projetId, ClaimsPrincipal utilisateurConnecte);
}
