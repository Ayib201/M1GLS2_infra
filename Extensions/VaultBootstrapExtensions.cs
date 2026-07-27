using M1GLS2_infra.Services;

namespace M1GLS2_infra.Extensions;

/// <summary>
/// Logique de démarrage qui doit s'exécuter AVANT que le conteneur
/// d'injection de dépendances (DI) n'existe -- donc avant `builder.Build()`.
/// </summary>
public static class VaultBootstrapExtensions
{
    /// <summary>
    /// S'authentifie auprès de Vault et récupère les chaînes de connexion
    /// PostgreSQL ET Redis.
    ///
    /// Pourquoi ici et pas comme les autres services (voir les autres classes
    /// du dossier Extensions/) ? `AddPostgresDatabase` et `AddRedisCache` ont
    /// besoin de ces chaînes de connexion pour enregistrer leurs services
    /// respectifs (`AddDbContext`, `AddStackExchangeRedisCache`) -- et ces
    /// enregistrements doivent se faire AVANT `builder.Build()`. Or
    /// `builder.Services` n'est qu'une LISTE de règles d'injection à ce
    /// stade : rien n'est encore "construit", donc impossible d'y résoudre
    /// `IVaultSecretService` normalement. On crée donc ici un client Vault
    /// autonome, à usage unique, avant même de commencer à déclarer les
    /// services.
    ///
    /// Redis n'a pas de mot de passe en mode dev (voir docker-compose.yml) :
    /// faire transiter son adresse par Vault n'est donc pas motivé par la
    /// confidentialité (comme pour PostgreSQL), mais par la cohérence -- UN
    /// SEUL endroit centralise la configuration de "où sont les services dont
    /// dépend l'API", plutôt que de la disperser entre Vault et appsettings.json.
    /// </summary>
    public static async Task<BootstrapResultat> BootstrapVaultAsync(this WebApplicationBuilder builder)
    {
        using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        var startupLogger = loggerFactory.CreateLogger("Startup");

        var vaultService = new VaultSecretService(
            builder.Configuration,
            loggerFactory.CreateLogger<VaultSecretService>());

        startupLogger.LogInformation("Démarrage : authentification auprès de Vault...");
        await vaultService.VerifyAuthenticationAsync();

        var databaseConnectionString = await vaultService.GetSecretValueAsync(
            secretPath: "database",
            secretKey: "ConnectionString");

        startupLogger.LogInformation("Vault OK : chaîne de connexion PostgreSQL récupérée.");

        var redisConnectionString = await vaultService.GetSecretValueAsync(
            secretPath: "redis",
            secretKey: "ConnectionString");

        startupLogger.LogInformation("Vault OK : chaîne de connexion Redis récupérée.");

        return new BootstrapResultat(databaseConnectionString, redisConnectionString);
    }
}

/// <summary>
/// Regroupe tout ce que le bootstrap Vault a récupéré, avant même que
/// l'injection de dépendances n'existe -- voir BootstrapVaultAsync ci-dessus.
/// </summary>
public sealed record BootstrapResultat(string DatabaseConnectionString, string RedisConnectionString);
