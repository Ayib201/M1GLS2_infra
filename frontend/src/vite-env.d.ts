/// <reference types="vite/client" />

// Étend le typage par défaut de Vite pour connaître NOS variables
// d'environnement précises (préfixées VITE_, seules exposées au navigateur).
// Sans ça, `import.meta.env.VITE_...` serait typé "any".
interface ImportMetaEnv {
  readonly VITE_KONG_BASE_URL: string;
  readonly VITE_KEYCLOAK_URL: string;
  readonly VITE_KEYCLOAK_REALM: string;
  readonly VITE_KEYCLOAK_CLIENT_ID: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
