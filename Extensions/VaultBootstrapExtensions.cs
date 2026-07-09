using M1GLS2_infra.Services;

namespace M1GLS2_infra.Extensions;

/// <summary>
/// Logique de démarrage qui doit s'exécuter AVANT que le conteneur
/// d'injection de dépendances (DI) n'existe -- donc avant `builder.Build()`.
/// </summary>
public static class VaultBootstrapExtensions
{
    /// <summary>
    /// S'authentifie auprès de Vault et récupère la chaîne de connexion
    /// PostgreSQL.
    ///
    /// Pourquoi ici et pas comme les autres services (voir les autres classes
    /// du dossier Extensions/) ? `AddPostgresDatabase` a besoin de cette chaîne de connexion pour
    /// enregistrer `AddDbContext` -- et cet enregistrement doit se faire
    /// AVANT `builder.Build()`. Or `builder.Services` n'est qu'une LISTE de
    /// règles d'injection à ce stade : rien n'est encore "construit", donc
    /// impossible d'y résoudre `IVaultSecretService` normalement. On crée
    /// donc ici un client Vault autonome, à usage unique, avant même de
    /// commencer à déclarer les services.
    /// </summary>
    public static async Task<string> BootstrapVaultAsync(this WebApplicationBuilder builder)
    {
        using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
        var startupLogger = loggerFactory.CreateLogger("Startup");

        var vaultService = new VaultSecretService(
            builder.Configuration,
            loggerFactory.CreateLogger<VaultSecretService>());

        startupLogger.LogInformation("Démarrage : authentification auprès de Vault...");
        await vaultService.VerifyAuthenticationAsync();

        var connectionString = await vaultService.GetSecretValueAsync(
            secretPath: "database",
            secretKey: "ConnectionString");

        startupLogger.LogInformation("Vault OK : chaîne de connexion PostgreSQL récupérée.");

        return connectionString;
    }
}
