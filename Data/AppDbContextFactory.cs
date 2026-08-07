using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace M1GLS2_infra.Data;

/// <summary>
/// Pourquoi une classe séparée ? "dotnet ef" a besoin de construire un
/// AppDbContext pour inspecter ton modèle (les entités) et générer le SQL
/// de migration -- mais sans démarrer toute l'application (pas
/// d'authentification Vault, pas de pipeline Kong/Keycloak, juste EF Core).
/// Sans cette fabrique, "dotnet ef" essaierait d'exécuter tout Program.cs.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Chaîne de connexion "locale" : correspond aux identifiants de démo
        // définis dans docker-compose.yml (service "postgres") et au port 5432
        // publié sur ta machine. Utilisée UNIQUEMENT par cet outil de migration.
        // L'application, elle, lira toujours la vraie chaîne de connexion
        // depuis Vault au démarrage (voir Program.cs) -- jamais celle-ci.
        const string designTimeConnectionString =
            "Host=localhost;Port=5432;Database=infra_demo;Username=infra_user;Password=infra_password_dev";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(designTimeConnectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
