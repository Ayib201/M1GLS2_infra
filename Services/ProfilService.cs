using System.Security.Claims;
using M1GLS2_infra.Data;
using M1GLS2_infra.Models;

namespace M1GLS2_infra.Services;

public sealed class ProfilService : IProfilService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<ProfilService> _logger;

    public ProfilService(AppDbContext dbContext, ILogger<ProfilService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ProfilCreationResultat> CreerProfilDepuisJetonAsync(ClaimsPrincipal utilisateurConnecte)
    {
        // Ces valeurs viennent directement des "claims" du jeton JWT, déjà
        // décodé et validé par le middleware d'authentification en amont
        // (voir KeycloakAuthenticationExtensions) -- rien n'est décodé ici.
        var email = utilisateurConnecte.FindFirstValue("email");
        var nom = utilisateurConnecte.FindFirstValue("preferred_username") ?? "Utilisateur inconnu";

        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("Tentative de création de profil sans email exploitable dans le jeton.");
            return ProfilCreationResultat.Echec("Le jeton Keycloak ne contient pas d'adresse email exploitable.");
        }

        var utilisateur = new Utilisateur
        {
            Nom = nom,
            Email = email
        };

        _dbContext.Utilisateurs.Add(utilisateur);
        await _dbContext.SaveChangesAsync();   // c'est ICI que le SQL INSERT part vers PostgreSQL

        _logger.LogInformation("Profil créé pour {Email}", email);

        return ProfilCreationResultat.Succes(utilisateur);
    }
}
