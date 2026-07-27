using M1GLS2_infra.Services;

namespace M1GLS2_infra.Extensions;

public static class RedisCacheServiceExtensions
{
    /// <summary>
    /// Enregistre Redis comme implémentation de IDistributedCache
    /// (AddStackExchangeRedisCache, fourni par Microsoft), puis ICacheService
    /// par-dessus (voir Services/CacheService.cs). "Singleton" pour les deux :
    /// la connexion Redis (ConnectionMultiplexer) est conçue pour être
    /// partagée et réutilisée par toute l'application, jamais recréée par
    /// requête -- contrairement à AppDbContext (Scoped), qui lui doit être
    /// une instance par requête.
    ///
    /// "connectionString" vient de Vault (voir VaultBootstrapExtensions.cs),
    /// pas de IConfiguration -- même principe que AddPostgresDatabase : la
    /// valeur est déjà résolue par le bootstrap AVANT que ce service ne soit
    /// enregistré, ce qui garde AddRedisCache totalement ignorant de VaultSharp.
    /// </summary>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, string connectionString)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            // Préfixe ajouté devant CHAQUE clé stockée dans Redis -- pratique
            // pour repérer immédiatement (ex: via `redis-cli keys "*"`) que ces
            // entrées viennent de cette application, si jamais Redis est un
            // jour partagé avec d'autres services.
            options.InstanceName = "infra-demo:";
        });

        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }
}
