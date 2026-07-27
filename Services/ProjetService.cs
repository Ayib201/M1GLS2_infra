using System.Security.Claims;
using M1GLS2_infra.Data;
using M1GLS2_infra.Models;
using Microsoft.EntityFrameworkCore;

namespace M1GLS2_infra.Services;

public sealed class ProjetService : IProjetService
{
    // Durée de vie du cache : après ce délai, Redis oublie l'entrée tout
    // seul et la prochaine lecture repart chercher les données en base --
    // filet de sécurité si jamais une invalidation était oubliée quelque part.
    private static readonly TimeSpan DureeCache = TimeSpan.FromSeconds(30);

    private readonly AppDbContext _dbContext;
    private readonly IUtilisateurCourantService _utilisateurCourantService;
    private readonly ICacheService _cacheService;

    public ProjetService(
        AppDbContext dbContext,
        IUtilisateurCourantService utilisateurCourantService,
        ICacheService cacheService)
    {
        _dbContext = dbContext;
        _utilisateurCourantService = utilisateurCourantService;
        _cacheService = cacheService;
    }

    public async Task<ResultatListeProjets> ListerMesProjetsAsync(ClaimsPrincipal utilisateurConnecte)
    {
        var utilisateur = await _utilisateurCourantService.ObtenirOuCreerAsync(utilisateurConnecte);
        var cle = ClePourListeProjets(utilisateur.Id);

        // "Cache-aside" : on regarde D'ABORD dans Redis. Si la donnée y est
        // déjà (un HIT), on la renvoie directement -- la base de données
        // n'est même pas sollicitée.
        var projetsEnCache = await _cacheService.ObtenirAsync<List<Projet>>(cle);
        if (projetsEnCache is not null)
        {
            return new ResultatListeProjets(projetsEnCache, ProvientDuCache: true);
        }

        // MISS : la donnée n'est pas (ou plus) en cache -- on va la chercher
        // en base, comme avant.
        //
        // Le Task.Delay ci-dessous SIMULE une base plus lente : avec
        // seulement quelques projets en local, PostgreSQL répond déjà en
        // 1-2 ms, ce qui rendrait l'apport du cache invisible en démo. Ce
        // délai artificiel rend la différence HIT/MISS visible à l'œil nu
        // (regarde le temps de réponse dans l'onglet Network du navigateur).
        // À retirer si ce projet devient autre chose qu'une démo.
        await Task.Delay(300);

        var projets = await _dbContext.Projets
            .Where(p => p.UtilisateurId == utilisateur.Id)
            .OrderByDescending(p => p.DateCreation)
            .ToListAsync();

        await _cacheService.DefinirAsync(cle, projets, DureeCache);

        return new ResultatListeProjets(projets, ProvientDuCache: false);
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

        // Invalidation : sans cette ligne, la liste en cache resterait
        // périmée jusqu'à expiration (30s) -- l'utilisateur ne verrait pas
        // son nouveau projet immédiatement après l'avoir créé.
        await _cacheService.SupprimerAsync(ClePourListeProjets(utilisateur.Id));

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

        // Même raison que dans CreerProjetAsync : la liste en cache doit
        // refléter la suppression dès maintenant, pas dans 30 secondes.
        await _cacheService.SupprimerAsync(ClePourListeProjets(utilisateur.Id));

        return true;
    }

    // Une clé par UTILISATEUR (jamais une clé globale "projets:tous") --
    // chacun ne doit voir que son propre cache, exactement comme chacun ne
    // voit que ses propres lignes en base (voir les Where(...UtilisateurId)
    // ci-dessus).
    private static string ClePourListeProjets(Guid utilisateurId) => $"projets:utilisateur:{utilisateurId}";
}
