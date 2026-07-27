namespace M1GLS2_infra.Services;

/// <summary>
/// Abstraction au-dessus de Redis (concrètement, IDistributedCache de
/// Microsoft). Même principe d'Inversion de Dépendances que
/// IVaultSecretService : les services métier (ex: ProjetService) dépendent
/// de ce CONTRAT, jamais directement de StackExchange.Redis. Si demain on
/// remplace Redis par un autre cache distribué, seule CacheService change.
/// </summary>
public interface ICacheService
{
    /// <summary>Retourne la valeur en cache, ou "default" (souvent null) si absente ou expirée.</summary>
    Task<T?> ObtenirAsync<T>(string cle, CancellationToken cancellationToken = default);

    /// <summary>Enregistre une valeur avec une durée de vie -- après "duree", Redis l'oublie tout seul.</summary>
    Task DefinirAsync<T>(string cle, T valeur, TimeSpan duree, CancellationToken cancellationToken = default);

    /// <summary>Retire explicitement une valeur (invalidation immédiate, ex: après une écriture).</summary>
    Task SupprimerAsync(string cle, CancellationToken cancellationToken = default);
}
