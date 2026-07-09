using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;

namespace M1GLS2_infra.Services;

/// <summary>
/// Implémentation concrète de <see cref="IVaultSecretService"/>, basée sur VaultSharp.
///
/// Cycle de vie : enregistrée en Singleton dans Program.cs. Le client Vault
/// (IVaultClient) est donc créé UNE SEULE FOIS et réutilisé pour toute la
/// durée de vie de l'application — on évite de rouvrir une connexion/authentification
/// à chaque requête HTTP, ce qui serait coûteux en performance.
/// </summary>
public sealed class VaultSecretService : IVaultSecretService
{
    // Point de montage du moteur de secrets "Key/Value version 2" dans Vault.
    // C'est la valeur par défaut quand on active `vault secrets enable -version=2 kv`
    // (ou, comme ici, le moteur "secret/" déjà activé par défaut en mode dev).
    private const string KeyValueMountPoint = "secret";

    private readonly IVaultClient _vaultClient;
    private readonly ILogger<VaultSecretService> _logger;

    public VaultSecretService(IConfiguration configuration, ILogger<VaultSecretService> logger)
    {
        _logger = logger;

        // On lit l'adresse et le token depuis la configuration (variables
        // d'environnement en Docker, voir docker-compose.yml) — JAMAIS en dur
        // dans le code ni dans appsettings.json commité.
        var vaultAddress = configuration["Vault:Address"]
            ?? throw new InvalidOperationException(
                "Configuration manquante : 'Vault:Address'. Vérifie la variable d'environnement Vault__Address.");

        var vaultToken = configuration["Vault:Token"]
            ?? throw new InvalidOperationException(
                "Configuration manquante : 'Vault:Token'. Vérifie la variable d'environnement Vault__Token.");

        // Authentification par token : la méthode la plus simple, adaptée au
        // mode dev de Vault. En production, on utiliserait plutôt AppRole ou
        // Kubernetes Auth, qui délivrent des tokens de courte durée de vie.
        IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);
        var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod);

        _vaultClient = new VaultClient(vaultClientSettings);
    }

    public async Task VerifyAuthenticationAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 5;
        var delayBetweenAttempts = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                // LookupSelf demande à Vault "qui suis-je avec ce token ?".
                // Si Vault répond, l'authentification est bien établie :
                // c'est notre étape "s'authentifier auprès de Vault au démarrage".
                var tokenInfo = await _vaultClient.V1.Auth.Token.LookupSelfAsync();

                _logger.LogInformation(
                    "Authentification Vault réussie. Policies attachées au token : {Policies}",
                    string.Join(", ", tokenInfo.Data.Policies ?? new List<string>()));
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                // Cas fréquent au tout premier démarrage : le conteneur Vault
                // n'a pas encore fini de démarrer que l'API essaie déjà de lui parler
                // (docker-compose "depends_on" ne garantit que l'ORDRE de démarrage,
                // pas la disponibilité réelle du service). On retente donc quelques
                // fois avant d'abandonner.
                _logger.LogWarning(ex,
                    "Échec de connexion à Vault (tentative {Attempt}/{MaxAttempts}). Nouvel essai dans {Delay}s...",
                    attempt, maxAttempts, delayBetweenAttempts.TotalSeconds);

                await Task.Delay(delayBetweenAttempts, cancellationToken);
            }
        }

        // Dernière tentative, sans filtre d'exception cette fois : si ça échoue
        // encore, on laisse l'exception remonter jusqu'à Program.cs, ce qui
        // empêche l'application de démarrer. Un crash net au démarrage vaut
        // mieux qu'une API qui répond des erreurs 500 en boucle sur Service A.
        await _vaultClient.V1.Auth.Token.LookupSelfAsync();
    }

    public async Task<string> GetSecretValueAsync(string secretPath, string secretKey, CancellationToken cancellationToken = default)
    {
        var secret = await _vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            path: secretPath,
            mountPoint: KeyValueMountPoint);

        if (!secret.Data.Data.TryGetValue(secretKey, out var value) || value is null)
        {
            throw new KeyNotFoundException(
                $"La clé '{secretKey}' est introuvable dans le secret Vault situé à '{KeyValueMountPoint}/{secretPath}'.");
        }

        return value.ToString()!;
    }
}
