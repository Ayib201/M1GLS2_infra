/**
 * Centralise la lecture des variables d'environnement (voir .env / vite-env.d.ts).
 * Un seul endroit à modifier si une URL change, et le typage évite les fautes
 * de frappe sur `import.meta.env.VITE_...` dispersées dans tout le code.
 */
export const config = {
  kongBaseUrl: import.meta.env.VITE_KONG_BASE_URL,
  keycloakUrl: import.meta.env.VITE_KEYCLOAK_URL,
  keycloakRealm: import.meta.env.VITE_KEYCLOAK_REALM,
  keycloakClientId: import.meta.env.VITE_KEYCLOAK_CLIENT_ID,
} as const;
