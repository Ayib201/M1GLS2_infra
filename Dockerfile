# =========================================================================
# Multi-stage build : on utilise DEUX images différentes.
# - "build" contient le SDK .NET complet (lourd, ~800 Mo) pour compiler.
# - "runtime" ne contient que ce qu'il faut pour EXÉCUTER l'application
#   (beaucoup plus léger). Seul le résultat de la compilation est copié
#   de l'étape "build" vers l'étape "runtime" : le SDK ne finit jamais
#   dans l'image finale. Image finale plus petite = déploiement plus
#   rapide et surface d'attaque réduite (moins d'outils présents = moins
#   de choses exploitables si le conteneur est compromis).
# =========================================================================

# ---------- Étape 1 : compilation ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# On copie d'abord uniquement le .csproj pour profiter du cache Docker :
# si seul le code change (pas les dépendances), "dotnet restore" n'est
# pas relancé à chaque build.
COPY M1GLS2_infra.csproj ./
RUN dotnet restore

# Puis on copie le reste du code source et on publie en mode Release.
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---------- Étape 2 : exécution ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Les images officielles .NET (depuis .NET 8) fournissent déjà un
# utilisateur non-root nommé "app". On l'utilise plutôt que de rester en
# "root" par défaut : principe du moindre privilège — si un attaquant
# exploite une faille dans l'appli, il n'obtient pas les droits root
# du conteneur.
USER app

EXPOSE 8080
ENTRYPOINT ["dotnet", "M1GLS2_infra.dll"]
