using M1GLS2_infra.Data;
using Microsoft.EntityFrameworkCore;

namespace M1GLS2_infra.Extensions;

public static class DatabaseServiceExtensions
{
    /// <summary>
    /// Enregistre <see cref="AppDbContext"/> (EF Core / PostgreSQL) en
    /// "Scoped" (comportement par défaut d'AddDbContext) : une instance par
    /// requête HTTP. C'est le mode recommandé pour EF Core -- une même
    /// instance de DbContext n'est pas conçue pour être partagée entre
    /// plusieurs requêtes en parallèle.
    /// </summary>
    public static IServiceCollection AddPostgresDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        return services;
    }
}
