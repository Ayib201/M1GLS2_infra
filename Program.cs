using M1GLS2_infra.Extensions;
using Serilog;

// Logger de "bootstrap" : actif dès cette toute première ligne, avant même
// que WebApplicationBuilder n'existe. Il capture les tout premiers logs
// (dont ceux du bootstrap Vault juste en dessous) et, grâce au try/catch
// plus bas, toute exception qui empêcherait l'application de démarrer --
// sans lui, un crash au tout début du démarrage ne laisserait AUCUNE trace.
// Reconfiguré (mais avec la MÊME configuration, voir ConfigurerSerilog) une
// fois que builder.Host existe, pour s'intégrer au reste de l'application
// (ILogger<T> injecté partout, etc.).
Log.Logger = new LoggerConfiguration()
    .ConfigurerSerilog()
    .CreateBootstrapLogger();

try
{
    Log.Information("Démarrage de l'application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((_, loggerConfiguration) => loggerConfiguration.ConfigurerSerilog());

    var bootstrap = await builder.BootstrapVaultAsync();

    // ---------------------------------------------------------------------
    // Déclaration des services : chaque ligne délègue à une classe dédiée
    // (dossier Extensions/), pour que ce fichier reste lisible d'un coup d'œil
    // -- Program.cs devient une simple "table des matières" de la configuration,
    // au lieu de tout contenir en un seul bloc.
    // ---------------------------------------------------------------------
    builder.Services.AddInfraCors();
    builder.Services.AddVaultSecretService();
    builder.Services.AddPostgresDatabase(bootstrap.DatabaseConnectionString);
    builder.Services.AddUtilisateurCourantService();
    builder.Services.AddRedisCache(bootstrap.RedisConnectionString);
    builder.Services.AddDomaineMetierServices();
    builder.Services.AddControllersSupport();
    builder.Services.AddSwaggerWithBearerAuth();
    builder.Services.AddKeycloakAuthentication(builder.Configuration);

    var app = builder.Build();

    // ---------------------------------------------------------------------
    // Pipeline HTTP (middlewares) : l'ORDRE de ces lignes compte, c'est la
    // chaîne que traverse chaque requête entrante, dans l'ordre où elle est
    // déclarée ici.
    // ---------------------------------------------------------------------
    app.UseGlobalExceptionHandling();

    // Une SEULE ligne de log structurée par requête HTTP (méthode, chemin,
    // code retour, durée), à la place du bruit verbeux par défaut d'ASP.NET
    // Core (plusieurs lignes par requête, une par middleware). Placé tôt
    // dans le pipeline pour mesurer la durée totale, y compris CORS/Auth.
    app.UseSerilogRequestLogging(options =>
    {
        // "EnrichDiagnosticContext" ajoute des propriétés à CETTE ligne de
        // log précise -- ici, deux informations qui n'existaient qu'à l'état
        // de header HTTP ou de claim JWT jusqu'ici, et qui deviennent
        // interrogeables dans les logs (ex: "toutes les requêtes de cet
        // utilisateur", "% de requêtes servies depuis le cache Redis").
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var emailUtilisateur = httpContext.User.FindFirst("email")?.Value;
            if (emailUtilisateur is not null)
            {
                diagnosticContext.Set("UtilisateurEmail", emailUtilisateur);
            }

            // Présent uniquement sur les endpoints de liste (Projets/Taches/
            // Commentaires) -- voir Controllers/ProjetsController.cs et les
            // deux autres contrôleurs. Absent partout ailleurs, ce qui est
            // normal (pas d'erreur si le header n'existe pas).
            if (httpContext.Response.Headers.TryGetValue("X-Cache-Status", out var statutCache))
            {
                diagnosticContext.Set("CacheStatus", statutCache.ToString());
            }
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(); // disponible sur /swagger
    }

    // Pas de UseHttpsRedirection() ici : le trafic chiffré serait terminé au
    // niveau de Kong ou d'un reverse-proxy en amont, pas sur chaque service interne.
    app.UseCors(CorsServiceExtensions.PolicyName);

    // Authentication (qui es-tu ?) doit toujours passer AVANT Authorization
    // (as-tu le droit ?).
    app.UseAuthentication();
    app.UseAuthorization();

    // ---------------------------------------------------------------------
    // Endpoints
    // ---------------------------------------------------------------------
    // Petit healthcheck, pratique pour vérifier que le conteneur de l'API tourne
    // indépendamment de Kong -- pas besoin d'un contrôleur pour ça.
    app.MapGet("/", () => Results.Ok(new { status = "InfraDemo API en cours d'exécution" }));

    // Projets, Taches et Commentaires sont des contrôleurs (voir dossier
    // Controllers/) : MapControllers() les découvre automatiquement grâce à
    // leurs attributs [ApiController]/[Route].
    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    // Attrape tout ce qui empêcherait l'application de démarrer (Vault
    // injoignable, configuration manquante...) -- SANS ce bloc, une telle
    // erreur afficherait juste une stack trace brute dans la console, non
    // structurée, et pourrait même passer inaperçue dans un environnement
    // où seuls les logs JSON sont surveillés.
    Log.Fatal(exception, "Arrêt anormal de l'application au démarrage.");
}
finally
{
    // Vide les logs en attente d'écriture avant que le processus ne se
    // termine -- Serilog peut mettre en mémoire tampon avant d'écrire.
    Log.CloseAndFlush();
}
