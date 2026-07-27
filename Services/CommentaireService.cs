using System.Security.Claims;
using M1GLS2_infra.Data;
using M1GLS2_infra.Models;
using Microsoft.EntityFrameworkCore;

namespace M1GLS2_infra.Services;

public sealed class CommentaireService : ICommentaireService
{
    // Même durée que ProjetService/TacheService.
    private static readonly TimeSpan DureeCache = TimeSpan.FromSeconds(30);

    private readonly AppDbContext _dbContext;
    private readonly IUtilisateurCourantService _utilisateurCourantService;
    private readonly ICacheService _cacheService;

    public CommentaireService(
        AppDbContext dbContext,
        IUtilisateurCourantService utilisateurCourantService,
        ICacheService cacheService)
    {
        _dbContext = dbContext;
        _utilisateurCourantService = utilisateurCourantService;
        _cacheService = cacheService;
    }

    public async Task<ResultatListeCommentaires?> ListerCommentairesAsync(
        Guid projetId, Guid tacheId, ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        var tacheAccessible = await TacheEstAccessibleAsync(projetId, tacheId, utilisateur.Id);
        if (!tacheAccessible)
        {
            return null;
        }

        // Cache-aside, clé par TÂCHE : seul le propriétaire du projet parent
        // peut jamais passer "tacheAccessible" ci-dessus, donc pas besoin
        // d'inclure l'utilisateur dans la clé.
        var cle = ClePourListeCommentaires(tacheId);

        var commentairesEnCache = await _cacheService.ObtenirAsync<List<Commentaire>>(cle);
        if (commentairesEnCache is not null)
        {
            return new ResultatListeCommentaires(commentairesEnCache, ProvientDuCache: true);
        }

        // SIMULATION pour la démo -- voir ProjetService pour le détail.
        await Task.Delay(300);

        var commentaires = await _dbContext.Commentaires
            .Where(c => c.TacheId == tacheId)
            .OrderBy(c => c.DateCreation)
            .ToListAsync();

        await _cacheService.DefinirAsync(cle, commentaires, DureeCache);

        return new ResultatListeCommentaires(commentaires, ProvientDuCache: false);
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

        await _cacheService.SupprimerAsync(ClePourListeCommentaires(tacheId));

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

        await _cacheService.SupprimerAsync(ClePourListeCommentaires(tacheId));

        return true;
    }

    private static string ClePourListeCommentaires(Guid tacheId) => $"commentaires:tache:{tacheId}";

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
