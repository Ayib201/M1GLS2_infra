using Microsoft.OpenApi.Models;

namespace M1GLS2_infra.Extensions;

public static class SwaggerServiceExtensions
{
    /// <summary>
    /// Documentation interactive de l'API (activée uniquement en
    /// Development, voir Program.cs), avec le bouton "Authorize" pour tester
    /// les endpoints protégés directement depuis le navigateur en collant un
    /// jeton Keycloak.
    /// </summary>
    public static IServiceCollection AddSwaggerWithBearerAuth(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "InfraDemo API",
                Version = "v1",
                Description = "Démo pédagogique : Kong + Vault + Keycloak + PostgreSQL + ASP.NET Core Minimal API"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Colle ici le jeton obtenu auprès de Keycloak (sans le mot \"Bearer\")."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
