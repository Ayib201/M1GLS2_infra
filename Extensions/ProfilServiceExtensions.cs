using M1GLS2_infra.Services;

namespace M1GLS2_infra.Extensions;

public static class ProfilServiceExtensions
{
    /// <summary>
    /// Enregistre la logique métier de création de profil. "Scoped" car
    /// <see cref="ProfilService"/> dépend d'AppDbContext, lui-même Scoped
    /// (une instance par requête) -- une dépendance ne peut pas avoir une
    /// durée de vie plus longue que ce dont elle dépend (Singleton dépendant
    /// d'un Scoped provoquerait une erreur au démarrage).
    /// </summary>
    public static IServiceCollection AddProfilService(this IServiceCollection services)
    {
        services.AddScoped<IProfilService, ProfilService>();
        return services;
    }
}
