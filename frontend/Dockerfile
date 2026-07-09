# =========================================================================
# Multi-stage build, même logique que le Dockerfile de l'API .NET :
# - "build" contient Node.js + npm (lourd) pour compiler le React/TypeScript
#   en fichiers statiques (HTML/CSS/JS).
# - "runtime" ne contient qu'un serveur web léger (Nginx) + ces fichiers
#   statiques. Node.js et tout le code source ne finissent JAMAIS dans
#   l'image finale.
# =========================================================================

# ---------- Étape 1 : build ----------
FROM node:22-alpine AS build
WORKDIR /app

# On copie d'abord uniquement les fichiers de dépendances pour profiter du
# cache Docker : si seul le code source change (pas package.json), "npm
# install" n'est pas relancé à chaque build.
COPY package.json ./
RUN npm install

COPY . .

# Important : les variables VITE_* (voir .env) sont injectées DANS le code
# JavaScript au moment du build (contrairement au backend .NET où Vault
# fournit la config au démarrage) -- une SPA statique n'a plus de "démarrage"
# après coup, juste des fichiers HTML/CSS/JS déjà figés. C'est pour ça que ce
# .env ne contient que des URLs publiques, jamais de secret : il finit
# littéralement dans le bundle JS téléchargeable par n'importe quel visiteur.
RUN npm run build

# ---------- Étape 2 : service des fichiers statiques ----------
FROM nginx:alpine AS runtime

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
