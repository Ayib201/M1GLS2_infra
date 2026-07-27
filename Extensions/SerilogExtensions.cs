using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace M1GLS2_infra.Extensions;

/// <summary>
/// Configuration centralisée de Serilog. "Logging structuré" veut dire que
/// chaque ligne de log est un objet JSON avec des champs nommés (Niveau,
/// Message, propriétés personnalisées...) plutôt qu'une simple phrase en
/// texte libre -- ça permet de FILTRER et RECHERCHER dans les logs (ex:
/// "montre-moi toutes les requêtes de tel utilisateur" ou "toutes celles
/// servies depuis le cache") au lieu de faire un grep approximatif sur du texte.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Construit la configuration Serilog "définitive" -- utilisée à la fois
    /// par le logger de bootstrap (avant que l'hôte n'existe, voir Program.cs)
    /// et par builder.Host.UseSerilog() une fois l'hôte disponible. Centraliser
    /// cette configuration ici évite d'avoir deux définitions différentes
    /// (une pour le bootstrap, une pour le "vrai" logger) qui pourraient
    /// diverger avec le temps.
    /// </summary>
    public static LoggerConfiguration ConfigurerSerilog(this LoggerConfiguration loggerConfiguration)
    {
        return loggerConfiguration
            .MinimumLevel.Information()
            // Les frameworks Microsoft (routage, EF Core SQL, etc.) sont très
            // bavards en Information -- on ne garde que Warning et plus grave
            // pour eux, comme le faisait déjà "Microsoft.AspNetCore": "Warning"
            // dans appsettings.json avant Serilog.
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            // Nécessaire pour que UseSerilogRequestLogging (voir Program.cs)
            // puisse ajouter des propriétés (UtilisateurEmail, CacheStatus...)
            // à la ligne de log de chaque requête via le "LogContext" ambiant.
            .Enrich.FromLogContext()
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            // Sortie sur la console (stdout du conteneur) : c'est la pratique
            // standard en environnement conteneurisé -- Docker/Kubernetes
            // collectent stdout tout seuls, pas besoin d'écrire dans un
            // fichier À L'INTÉRIEUR du conteneur (perdu au prochain redémarrage).
            // CompactJsonFormatter = une ligne JSON par événement.
            .WriteTo.Console(new CompactJsonFormatter());
    }
}
