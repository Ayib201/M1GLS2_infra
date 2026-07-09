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
    /// </summary>
    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloakAuthority = configuration["Keycloak:Authority"]
            ?? throw new InvalidOperationException("Configuration manquante : 'Keycloak:Authority'.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakAuthority;

                // Keycloak tourne en HTTP en local (pas de certificat TLS dans
                // cette démo) -- à interdire en production, où l'Authority DOIT être en HTTPS.
                options.RequireHttpsMetadata = false;

                // Empêche ASP.NET Core de renommer certains claims JWT standards
                // vers d'anciennes URI Microsoft (ex: "sub" -> ClaimTypes.NameIdentifier).
                // On garde les noms de claims EXACTEMENT tels que Keycloak les émet.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,          // le jeton doit venir de CE realm Keycloak précis
                    ValidateLifetime = true,        // refuse les jetons expirés
                    ValidateIssuerSigningKey = true // vérifie la signature cryptographique du jeton
                    // ValidateAudience volontairement omis (donc désactivé) : simplification
                    // de démo -- Keycloak ne place "infra-api" dans le claim "aud" que si on
                    // ajoute un "audience mapper" dédié sur le client. En prod : ValidateAudience=true
                    // + mapper explicite dans keycloak/realm-export.json.
                };
            });

        services.AddAuthorization();

        return services;
    }
}
