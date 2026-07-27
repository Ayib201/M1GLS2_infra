using M1GLS2_infra.Extensions;

var builder = WebApplication.CreateBuilder(args);

var databaseConnectionString = await builder.BootstrapVaultAsync();

// ---------------------------------------------------------------------
// Déclaration des services : chaque ligne délègue à une classe dédiée
// (dossier Extensions/), pour que ce fichier reste lisible d'un coup d'œil
// -- Program.cs devient une simple "table des matières" de la configuration,
// au lieu de tout contenir en un seul bloc.
// ---------------------------------------------------------------------
builder.Services.AddInfraCors();
builder.Services.AddVaultSecretService();
builder.Services.AddPostgresDatabase(databaseConnectionString);
builder.Services.AddUtilisateurCourantService();
builder.Services.AddRedisCache(builder.Configuration);
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
