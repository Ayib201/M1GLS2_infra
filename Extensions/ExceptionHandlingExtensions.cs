using Microsoft.AspNetCore.Diagnostics;

namespace M1GLS2_infra.Extensions;

public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Intercepte TOUTE exception non gérée (ex: PostgreSQL ou Keycloak
    /// injoignable en cours de route), la journalise en détail côté serveur
    /// (pour le débogage), et renvoie au client une réponse JSON
    /// volontairement sobre -- jamais de stack trace exposée, pour ne pas
    /// révéler de détails internes à un attaquant.
    ///
    /// "logger.LogError" ci-dessous n'a jamais eu besoin de changer pour
    /// profiter de Serilog (voir Program.cs/SerilogExtensions.cs) :
    /// `ILogger&lt;Program&gt;` est une abstraction Microsoft, Serilog vient
    /// simplement s'y brancher comme fournisseur -- toute exception logguée
    /// ici sort donc désormais en JSON structuré, sans avoir touché cette ligne.
    /// </summary>
    public static WebApplication UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

                if (exceptionFeature is not null)
                {
                    logger.LogError(exceptionFeature.Error,
                        "Erreur non gérée sur {Path}", exceptionFeature.Path);
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Une erreur interne est survenue. Réessaie plus tard."
                });
            });
        });

        return app;
    }
}
