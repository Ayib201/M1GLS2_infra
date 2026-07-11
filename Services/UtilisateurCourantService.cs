using System.Security.Claims;
using M1GLS2_infra.Data;
using M1GLS2_infra.Models;
using Microsoft.EntityFrameworkCore;

namespace M1GLS2_infra.Services;

public sealed class UtilisateurCourantService : IUtilisateurCourantService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UtilisateurCourantService> _logger;

    public UtilisateurCourantService(AppDbContext dbContext, ILogger<UtilisateurCourantService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Utilisateur> ObtenirOuCreerAsync(ClaimsPrincipal utilisateurConnecte)
    {
        var email = utilisateurConnecte.FindFirstValue("email");

        if (string.IsNullOrWhiteSpace(email))
        {
            // Cas anormal : si Keycloak est correctement configuré (voir
            // realm-export.json), un jeton valide contient toujours un email.
            // On laisse le middleware global (UseGlobalExceptionHandling)
            // transformer ceci en 500 -- ce n'est pas une erreur "normale"
            // côté utilisateur, mais un problème de configuration.
            throw new InvalidOperationException(
                "Le jeton Keycloak ne contient pas d'adresse email exploitable.");
        }

        var utilisateurExistant = await _dbContext.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == email);

        if (utilisateurExistant is not null)
        {
            return utilisateurExistant;
        }

        var nom = utilisateurConnecte.FindFirstValue("preferred_username") ?? "Utilisateur inconnu";

        var nouvelUtilisateur = new Utilisateur
        {
            Nom = nom,
            Email = email
        };

        _dbContext.Utilisateurs.Add(nouvelUtilisateur);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Profil auto-provisionné pour {Email}", email);

        return nouvelUtilisateur;
    }
}
