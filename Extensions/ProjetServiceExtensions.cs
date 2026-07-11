using M1GLS2_infra.Services;

namespace M1GLS2_infra.Extensions;

/// <summary>
/// Enregistre les 3 services métier (Projet/Tache/Commentaire). Tous
/// "Scoped" pour la même raison que IProfilService auparavant : ils
/// dépendent d'AppDbContext (lui-même Scoped, une instance par requête
/// HTTP).
/// </summary>
public static class ProjetServiceExtensions
{
    public static IServiceCollection AddDomaineMetierServices(this IServiceCollection services)
    {
        services.AddScoped<IProjetService, ProjetService>();
        services.AddScoped<ITacheService, TacheService>();
        services.AddScoped<ICommentaireService, CommentaireService>();
        return services;
    }
}
