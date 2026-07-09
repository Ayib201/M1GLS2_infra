using M1GLS2_infra.Services;

namespace M1GLS2_infra.Extensions;

public static class VaultServiceExtensions
{
    /// <summary>
    /// Enregistre le client Vault utilisé PENDANT toute la vie de
    /// l'application (à ne pas confondre avec le client "bootstrap" à usage
    /// unique de <see cref="VaultBootstrapExtensions"/>, utilisé uniquement
    /// avant le démarrage pour récupérer la chaîne de connexion PostgreSQL).
    ///
    /// Singleton : une seule instance, une seule authentification, réutilisée
    /// à chaque appel de /api/v1/serviceA.
    /// </summary>
    public static IServiceCollection AddVaultSecretService(this IServiceCollection services)
    {
        services.AddSingleton<IVaultSecretService, VaultSecretService>();
        return services;
    }
}
