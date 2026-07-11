using M1GLS2_infra.Services;

namespace M1GLS2_infra.Extensions;

public static class UtilisateurCourantServiceExtensions
{
    public static IServiceCollection AddUtilisateurCourantService(this IServiceCollection services)
    {
        services.AddScoped<IUtilisateurCourantService, UtilisateurCourantService>();
        return services;
    }
}
