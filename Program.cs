using M1GLS2_infra.Services;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// 1) Déclaration des services (injection de dépendances)
// =====================================================================
// L'injection de dépendances (DI) = au lieu qu'une classe crée elle-même
// ses outils (ex: `new VaultSecretService()`), on les déclare ici une fois,
// et ASP.NET Core les "injecte" automatiquement partout où on en a besoin
// (via le constructeur). Ça facilite les tests (on peut injecter une fausse
// implémentation) et respecte le principe d'Inversion de Dépendances.

// --- CORS ---
// CORS (Cross-Origin Resource Sharing) : par défaut, un navigateur bloque
// les appels JS faits depuis un site A vers une API sur un site B.
// On autorise ici TOUTES les origines, pour préparer l'intégration future
// d'un front Angular (qui tournera sur un autre port/domaine).
// ⚠️ En production, on restreindrait à la liste précise des domaines autorisés.
const string CorsPolicyName = "AllowAnyOrigin";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- HashiCorp Vault ---
// Singleton = une seule instance pour toute la durée de vie de l'application.
// Le client Vault (authentifié une fois) est donc réutilisé pour chaque appel,
// au lieu de se ré-authentifier à chaque requête HTTP.
builder.Services.AddSingleton<IVaultSecretService, VaultSecretService>();

// --- Swagger (uniquement utile en Development) ---
// Swagger génère une documentation interactive de l'API (page web où l'on
// peut tester les endpoints directement dans le navigateur), très utile en
// développement. On ne l'active PAS en production pour ne pas exposer
// publiquement le détail de tous les endpoints.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "InfraDemo API",
        Version = "v1",
        Description = "Démo pédagogique : Kong API Gateway + HashiCorp Vault + ASP.NET Core Minimal API"
    });
});

var app = builder.Build();

// =====================================================================
// 2) Vérification de la connexion à Vault, AU DÉMARRAGE
// =====================================================================
// C'est le point clé demandé : avant d'accepter le moindre trafic HTTP,
// l'application s'authentifie auprès de Vault. Si ça échoue, l'application
// ne démarre pas (voir la logique de retry + exception dans VaultSecretService).
using (var startupScope = app.Services.CreateScope())
{
    var vaultSecretService = startupScope.ServiceProvider.GetRequiredService<IVaultSecretService>();
    var startupLogger = startupScope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    startupLogger.LogInformation("Démarrage : authentification auprès de Vault...");
    await vaultSecretService.VerifyAuthenticationAsync();
    startupLogger.LogInformation("Vault OK, l'application peut démarrer.");
}

// =====================================================================
// 3) Pipeline HTTP (middlewares)
// =====================================================================
// Un "middleware" est un maillon de la chaîne de traitement d'une requête
// HTTP : chaque requête entrante passe par ces étapes, dans l'ordre où
// elles sont déclarées ci-dessous.

// --- Gestion globale des exceptions ---
// Sans ça, une exception non gérée dans un endpoint (ex: Vault injoignable
// pendant l'exécution) remonterait telle quelle au client : code 500 "brut",
// potentiellement avec la stack trace complète en mode Development. C'est un
// risque de sécurité (on révèle des détails internes à un attaquant) et une
// mauvaise expérience pour un futur client (Angular) qui reçoit un format
// d'erreur imprévisible. Ce middleware intercepte TOUTE exception non gérée,
// la journalise en détail côté serveur (pour le débogage), et renvoie au
// client une réponse JSON volontairement sobre.
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
            // Volontairement pas de message technique ni de stack trace ici :
            // le détail reste dans les logs serveur, jamais dans la réponse HTTP.
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // disponible sur /swagger
}

// Pas de UseHttpsRedirection() ici : dans ce projet de démo, le trafic
// chiffré (HTTPS) serait terminé au niveau de Kong ou d'un reverse-proxy
// en amont, pas sur chaque service interne. Entre les conteneurs Docker
// (Kong -> API -> Vault), le réseau est privé et non exposé directement.
app.UseCors(CorsPolicyName);

// =====================================================================
// 4) Endpoints
// =====================================================================
// Un petit endpoint racine, pratique pour vérifier rapidement (curl, navigateur)
// que le conteneur de l'API tourne, indépendamment de Kong.
app.MapGet("/", () => Results.Ok(new { status = "InfraDemo API en cours d'exécution" }));

// MapGroup regroupe des endpoints qui partagent un préfixe d'URL commun
// (/api/v1) et, optionnellement, des métadonnées communes (ici un tag
// Swagger). Ça évite de répéter "/api/v1/..." devant chaque route.
var apiV1 = app.MapGroup("/api/v1").WithTags("Services simulés");

// --- Service A : lit le secret dans Vault à CHAQUE appel ---
// (le client VaultSharp, lui, a été authentifié UNE SEULE FOIS au démarrage
// et est réutilisé ici — voir VaultSecretService). Lire le secret à chaque
// requête plutôt que de le mettre en cache permet de voir un changement de
// secret dans Vault se répercuter immédiatement, sans redémarrer l'API :
// c'est tout l'intérêt d'une gestion centralisée des secrets.
apiV1.MapGet("/serviceA", async (IVaultSecretService vaultSecretService) =>
{
    var secretValue = await vaultSecretService.GetSecretValueAsync(
        secretPath: "external-api",
        secretKey: "CleSecreteExterne");

    return Results.Ok(new
    {
        service = "A",
        secretRecupere = secretValue
    });
})
.WithName("ServiceA")
.Produces(StatusCodes.Status200OK);

// --- Service B : aucune dépendance externe, logique 100% bouchonnée ---
apiV1.MapGet("/serviceB", () =>
{
    return Results.Ok(new
    {
        service = "B",
        message = "Statut OK"
    });
})
.WithName("ServiceB")
.Produces(StatusCodes.Status200OK);

app.Run();
