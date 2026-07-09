namespace M1GLS2_infra.Services;

/// <summary>
/// Abstraction au-dessus de HashiCorp Vault.
///
/// Pourquoi une interface et pas directement VaultSharp partout ?
/// C'est le principe d'Inversion de Dépendances (le "D" de SOLID) :
/// les endpoints de l'API dépendent de ce CONTRAT, jamais d'une implémentation
/// concrète. Si demain on remplace Vault par Azure Key Vault ou AWS Secrets
/// Manager, seule <see cref="VaultSecretService"/> change — aucun endpoint
/// n'a besoin d'être modifié.
/// </summary>
public interface IVaultSecretService
{
    /// <summary>
    /// Vérifie, au démarrage de l'application, que l'authentification auprès
    /// de Vault fonctionne réellement (et pas seulement que la configuration
    /// est présente). Si Vault est injoignable ou le token invalide, cette
    /// méthode lève une exception : l'application doit s'arrêter plutôt que
    /// démarrer "à moitié cassée" (principe fail-fast).
    /// </summary>
    Task VerifyAuthenticationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lit une clé précise dans un secret Vault (moteur KV version 2).
    /// </summary>
    /// <param name="secretPath">Chemin du secret, ex: "external-api".</param>
    /// <param name="secretKey">Nom de la clé à l'intérieur du secret, ex: "CleSecreteExterne".</param>
    Task<string> GetSecretValueAsync(string secretPath, string secretKey, CancellationToken cancellationToken = default);
}
