namespace M1GLS2_infra.Extensions;

/// <summary>
/// Configuration CORS (Cross-Origin Resource Sharing) : par défaut, un
/// navigateur bloque les appels JS faits depuis un site A vers une API sur
/// un site B. Cette classe autorise le futur front Angular (sur un autre
/// port/domaine) à appeler cette API.
/// </summary>
public static class CorsServiceExtensions
{
    /// <summary>Nom de la policy, réutilisé dans Program.cs pour app.UseCors(...).</summary>
    public const string PolicyName = "AllowAnyOrigin";

    public static IServiceCollection AddInfraCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                // ⚠️ Autorise TOUTES les origines : pratique en développement/démo,
                // à restreindre à une liste précise de domaines en production.
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }
}
