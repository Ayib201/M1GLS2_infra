using System.Security.Claims;
using M1GLS2_infra.Data;
using M1GLS2_infra.Models;
using Microsoft.EntityFrameworkCore;

namespace M1GLS2_infra.Services;

public sealed class TacheService : ITacheService
{
    private readonly AppDbContext _dbContext;
    private readonly IUtilisateurCourantService _utilisateurCourantService;

    public TacheService(AppDbContext dbContext, IUtilisateurCourantService utilisateurCourantService)
    {
        _dbContext = dbContext;
        _utilisateurCourantService = utilisateurCourantService;
    }

    public async Task<IReadOnlyList<Tache>?> ListerTachesAsync(Guid projetId, ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        var projetAccessible = await _dbContext.Projets
            .AnyAsync(p => p.Id == projetId && p.UtilisateurId == utilisateur.Id);

        if (!projetAccessible)
        {
            return null;
        }

        return await _dbContext.Taches
            .Where(t => t.ProjetId == projetId)
            .OrderBy(t => t.DateCreation)
            .ToListAsync();
    }

    public async Task<Tache?> CreerTacheAsync(
        Guid projetId, CreerTacheRequest requete, ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        var projetAccessible = await _dbContext.Projets
            .AnyAsync(p => p.Id == projetId && p.UtilisateurId == utilisateur.Id);

        if (!projetAccessible)
        {
            return null;
        }

        var tache = new Tache
        {
            Titre = requete.Titre,
            Description = requete.Description,
            DateEcheance = requete.DateEcheance,
            ProjetId = projetId
        };

        _dbContext.Taches.Add(tache);
        await _dbContext.SaveChangesAsync();

        return tache;
    }

    public async Task<Tache?> MettreAJourTacheAsync(
        Guid projetId, Guid tacheId, MettreAJourTacheRequest requete, ClaimsPrincipal utilisateurConnecte)
    {
        var tache = await ObtenirTacheAccessibleAsync(projetId, tacheId, utilisateurConnecte);

        if (tache is null)
        {
            return null;
        }

        tache.Titre = requete.Titre;
        tache.Description = requete.Description;
        tache.Statut = requete.Statut;
        tache.DateEcheance = requete.DateEcheance;

        await _dbContext.SaveChangesAsync();

        return tache;
    }

    public async Task<bool> SupprimerTacheAsync(Guid projetId, Guid tacheId, ClaimsPrincipal utilisateurConnecte)
    {
        var tache = await ObtenirTacheAccessibleAsync(projetId, tacheId, utilisateurConnecte);

        if (tache is null)
        {
            return false;
        }

        _dbContext.Taches.Remove(tache);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Centralise la vérification "cette tâche existe, appartient bien au
    /// projet indiqué dans l'URL, et ce projet appartient à l'appelant" --
    /// réutilisée par MettreAJourTacheAsync et SupprimerTacheAsync pour ne
    /// pas dupliquer cette logique de sécurité.
    /// </summary>
    private async Task<Tache?> ObtenirTacheAccessibleAsync(
        Guid projetId, Guid tacheId, ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);

        // Pas de .Include(t => t.Projet) ici : on a seulement besoin de la
        // valeur UtilisateurId du projet parent pour FILTRER (EF Core la
        // traduit en simple jointure SQL), pas de charger et suivre l'objet
        // Projet en entier. Include() aurait attaché Projet au graphe suivi
        // par EF Core, qui aurait alors ré-attaché cette même Tache dans
        // Projet.Taches (comportement de "fixup") -- un cycle Tache -> Projet
        // -> Taches -> Tache que le sérialiseur JSON devrait ensuite gérer.
        return await _dbContext.Taches
            .FirstOrDefaultAsync(t =>
                t.Id == tacheId &&
                t.ProjetId == projetId &&
                t.Projet!.UtilisateurId == utilisateur.Id);
    }
}
