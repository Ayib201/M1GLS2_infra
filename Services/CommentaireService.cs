using System.Security.Claims;
using M1GLS2_infra.Data;
using M1GLS2_infra.Models;
using Microsoft.EntityFrameworkCore;

namespace M1GLS2_infra.Services;

public sealed class CommentaireService : ICommentaireService
{
    private readonly AppDbContext _dbContext;
    private readonly IUtilisateurCourantService _utilisateurCourantService;

    public CommentaireService(AppDbContext dbContext, IUtilisateurCourantService utilisateurCourantService)
    {
        _dbContext = dbContext;
        _utilisateurCourantService = utilisateurCourantService;
    }

    public async Task<IReadOnlyList<Commentaire>?> ListerCommentairesAsync(
        Guid projetId, Guid tacheId, ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        var tacheAccessible = await TacheEstAccessibleAsync(projetId, tacheId, utilisateur.Id);
        if (!tacheAccessible)
        {
            return null;
        }

        return await _dbContext.Commentaires
            .Where(c => c.TacheId == tacheId)
            .OrderBy(c => c.DateCreation)
            .ToListAsync();
    }

    public async Task<Commentaire?> CreerCommentaireAsync(
        Guid projetId, Guid tacheId, CreerCommentaireRequest requete, ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        var tacheAccessible = await TacheEstAccessibleAsync(projetId, tacheId, utilisateur.Id);
        if (!tacheAccessible)
        {
            return null;
        }

        var commentaire = new Commentaire
        {
            Contenu = requete.Contenu,
            TacheId = tacheId,
            UtilisateurId = utilisateur.Id
        };

        _dbContext.Commentaires.Add(commentaire);
        await _dbContext.SaveChangesAsync();

        return commentaire;
    }

    public async Task<bool> SupprimerCommentaireAsync(
        Guid projetId, Guid tacheId, Guid commentaireId, ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        var commentaire = await _dbContext.Commentaires
            .Include(c => c.Tache)
            .ThenInclude(t => t!.Projet)
            .FirstOrDefaultAsync(c =>
                c.Id == commentaireId &&
                c.TacheId == tacheId &&
                c.Tache!.ProjetId == projetId &&
                c.Tache.Projet!.UtilisateurId == utilisateur.Id);

        if (commentaire is null)
        {
            return false;
        }

        _dbContext.Commentaires.Remove(commentaire);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Vérifie que la tâche existe, appartient au projet indiqué, et que ce
    /// projet appartient bien à l'utilisateur -- toute la chaîne de
    /// propriété est vérifiée en une seule requête.
    /// </summary>
    private async Task<bool> TacheEstAccessibleAsync(Guid projetId, Guid tacheId, Guid utilisateurId)
    {
        return await _dbContext.Taches
            .AnyAsync(t =>
                t.Id == tacheId &&
                t.ProjetId == projetId &&
                t.Projet!.UtilisateurId == utilisateurId);
    }
}
