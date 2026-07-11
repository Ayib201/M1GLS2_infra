using System.Security.Claims;
using M1GLS2_infra.Data;
using M1GLS2_infra.Models;
using Microsoft.EntityFrameworkCore;

namespace M1GLS2_infra.Services;

public sealed class ProjetService : IProjetService
{
    private readonly AppDbContext _dbContext;
    private readonly IUtilisateurCourantService _utilisateurCourantService;

    public ProjetService(AppDbContext dbContext, IUtilisateurCourantService utilisateurCourantService)
    {
        _dbContext = dbContext;
        _utilisateurCourantService = utilisateurCourantService;
    }

    public async Task<IReadOnlyList<Projet>> ListerMesProjetsAsync(ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        // Filtre TOUJOURS sur l'ID résolu depuis le jeton, jamais sur un ID
        // envoyé par le client -- c'est ce qui garantit qu'un utilisateur ne
        // peut jamais lister les projets d'un autre.
        return await _dbContext.Projets
            .Where(p => p.UtilisateurId == utilisateur.Id)
            .OrderByDescending(p => p.DateCreation)
            .ToListAsync();
    }

    public async Task<Projet> CreerProjetAsync(CreerProjetRequest requete, ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        var projet = new Projet
        {
            Nom = requete.Nom,
            Description = requete.Description,
            UtilisateurId = utilisateur.Id
        };

        _dbContext.Projets.Add(projet);
        await _dbContext.SaveChangesAsync();

        return projet;
    }

    public async Task<Projet?> ObtenirProjetAsync(Guid projetId, ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        return await _dbContext.Projets
            .FirstOrDefaultAsync(p => p.Id == projetId && p.UtilisateurId == utilisateur.Id);
    }

    public async Task<bool> SupprimerProjetAsync(Guid projetId, ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        var projet = await _dbContext.Projets
            .FirstOrDefaultAsync(p => p.Id == projetId && p.UtilisateurId == utilisateur.Id);

        if (projet is null)
        {
            return false;
        }

        // La suppression en cascade (configurée dans AppDbContext) supprime
        // aussi automatiquement ses tâches et leurs commentaires.
        _dbContext.Projets.Remove(projet);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
