using System.Text.Json.Serialization;

namespace M1GLS2_infra.Extensions;

public static class ControllersServiceExtensions
{
    /// <summary>
    /// Active le support des contrôleurs MVC (attributs [ApiController],
    /// [Route], [HttpGet]/[HttpPost], [Authorize]...). Nécessaire pour que
    /// les contrôleurs du dossier Controllers/ soient détectés et exposés
    /// comme endpoints HTTP.
    /// </summary>
    public static IServiceCollection AddControllersSupport(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            // StatutTache (enum C#) sérialisé en texte lisible ("AFaire",
            // "EnCours"...) plutôt qu'en nombre opaque (0, 1, 2) -- plus
            // simple et plus sûr à consommer côté front TypeScript.
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            // Filet de sécurité : si une entité EF Core chargée avec
            // .Include() est un jour renvoyée directement (ex: Tache.Projet
            // qui référence en retour ses propres Taches), le sérialiseur
            // ignore le cycle au lieu de planter avec une exception au
            // moment de générer la réponse HTTP.
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
        return services;
    }
}
