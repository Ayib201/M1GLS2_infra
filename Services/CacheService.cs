using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace M1GLS2_infra.Services;

/// <summary>
/// Implémentation de ICacheService par-dessus IDistributedCache. Ce dernier
/// ne sait manipuler que des chaînes/octets -- cette classe s'occupe de
/// sérialiser/désérialiser en JSON, pour que le reste du code manipule de
/// vrais objets C# (List&lt;Projet&gt;, etc.) sans jamais toucher à du JSON brut.
/// </summary>
public sealed class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;

    public CacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> ObtenirAsync<T>(string cle, CancellationToken cancellationToken = default)
    {
        var valeurBrute = await _distributedCache.GetStringAsync(cle, cancellationToken);

        if (valeurBrute is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(valeurBrute);
    }

    public async Task DefinirAsync<T>(string cle, T valeur, TimeSpan duree, CancellationToken cancellationToken = default)
    {
        var valeurBrute = JsonSerializer.Serialize(valeur);

        await _distributedCache.SetStringAsync(
            cle,
            valeurBrute,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = duree },
            cancellationToken);
    }

    public Task SupprimerAsync(string cle, CancellationToken cancellationToken = default)
    {
        return _distributedCache.RemoveAsync(cle, cancellationToken);
    }
}
