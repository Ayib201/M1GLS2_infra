using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace M1GLS2_infra.Extensions;

public static class KeycloakAuthenticationExtensions
{
    /// <summary>
    /// Configure la validation des jetons JWT émis par Keycloak.
    ///
    /// C'est l'équivalent, pour les Minimal API, de ce que ferait [Authorize]
    /// sur un contrôleur MVC classique : ici, la protection se pose avec
    /// `.RequireAuthorization()` sur chaque endpoint (voir Endpoints/ProfilsEndpoints.cs).
    /// Le comportement est identique -- 401 si le jeton est absent/invalide --
    /// seule la syntaxe change.
    ///
    /// Particularité de cette configuration : Keycloak est joignable sous
    /// DEUX adresses différentes selon qui lui parle -- "localhost:8000"
    /// (via Kong) pour le navigateur, "keycloak:8080" en direct pour les
    /// autres conteneurs du réseau Docker (voir docker-compose.yml). Les
    /// jetons contiennent la première (c'est ce que Keycloak connaît de
    /// lui-même) ; l'API doit utiliser la seconde pour réellement lui parler
    /// sur le réseau. <see cref="KeycloakInternalRoutingHandler"/> fait ce
    /// pont automatiquement.
    /// </summary>
    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var internalAuthority = configuration["Keycloak:InternalAuthority"]
            ?? throw new InvalidOperationException("Configuration manquante : 'Keycloak:InternalAuthority'.");

        var publicIssuer = configuration["Keycloak:PublicIssuer"]
            ?? throw new InvalidOperationException("Configuration manquante : 'Keycloak:PublicIssuer'.");

        var clientId = configuration["Keycloak:ClientId"]
            ?? throw new InvalidOperationException("Configuration manquante : 'Keycloak:ClientId'.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // On indique explicitement où récupérer le document de découverte
                // OIDC (adresse INTERNE, toujours joignable depuis ce conteneur) --
                // plutôt que de laisser ASP.NET Core la déduire automatiquement
                // d'une "Authority" qui, elle, devrait être l'adresse PUBLIQUE.
                options.MetadataAddress = $"{internalAuthority}/.well-known/openid-configuration";

                // Keycloak tourne en HTTP en local (pas de certificat TLS dans
                // cette démo) -- à interdire en production, où tout DOIT être en HTTPS.
                options.RequireHttpsMetadata = false;

                // Empêche ASP.NET Core de renommer certains claims JWT standards
                // vers d'anciennes URI Microsoft (ex: "sub" -> ClaimTypes.NameIdentifier).
                // On garde les noms de claims EXACTEMENT tels que Keycloak les émet.
                options.MapInboundClaims = false;

                // Le document de découverte lui-même contient des URLs (notamment
                // "jwks_uri", pour vérifier la signature des jetons) construites
                // à partir de l'adresse PUBLIQUE de Keycloak -- injoignable depuis
                // ce conteneur. Ce handler réécrit ces appels vers l'adresse interne,
                // de façon totalement transparente pour le reste de la configuration.
                options.BackchannelHttpHandler = new KeycloakInternalRoutingHandler(publicIssuer, internalAuthority);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = publicIssuer,     // valeur RÉELLEMENT contenue dans les jetons émis
                    ValidateLifetime = true,        // refuse les jetons expirés
                    ValidateIssuerSigningKey = true, // vérifie la signature cryptographique du jeton

                    // IMPORTANT : par défaut, TokenValidationParameters.ValidateAudience vaut
                    // déjà TRUE même quand on ne l'écrit pas explicitement -- ce n'était PAS
                    // désactivé avant (contrairement à ce que suggérait un commentaire ici),
                    // ce qui rejetait TOUT jeton avec l'erreur "The audience 'empty' is invalid"
                    // dès qu'un vrai jeton Keycloak (sans claim "aud" correspondant) était utilisé.
                    // On active donc l'audience EXPLICITEMENT, et on exige la bonne valeur :
                    // "infra-api" doit apparaître dans le claim "aud" du jeton, ce que Keycloak ne
                    // fait que grâce au mapper "audience-infra-api" ajouté sur le client (voir
                    // keycloak/realm-export.json). Vérifier l'audience empêche un jeton émis pour
                    // une AUTRE application du même serveur Keycloak d'être rejoué contre cette API.
                    ValidateAudience = true,
                    ValidAudience = clientId
                };
            });

        services.AddAuthorization();

        return services;
    }
}
