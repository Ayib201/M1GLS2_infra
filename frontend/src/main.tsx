import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App";
import { keycloak } from "./auth/keycloak";

const rootElement = document.getElementById("root");
if (!rootElement) {
  throw new Error("Élément #root introuvable dans index.html.");
}

// On n'affiche l'application QU'APRÈS que keycloak-js ait tranché la
// question de l'authentification. `onLoad: "login-required"` = si
// l'utilisateur n'a pas de session valide, keycloak-js redirige
// automatiquement le navigateur vers la page de connexion hébergée par
// Keycloak -- notre code React n'a même pas besoin de gérer ce cas.
// `pkceMethod: "S256"` active PKCE (Proof Key for Code Exchange), une
// protection supplémentaire du flux "Authorization Code" recommandée pour
// toute application qui tourne dans un navigateur (SPA).
keycloak
  .init({ onLoad: "login-required", pkceMethod: "S256" })
  .then((authenticated) => {
    if (!authenticated) {
      throw new Error("Authentification Keycloak impossible.");
    }

    createRoot(rootElement).render(
      <StrictMode>
        <App />
      </StrictMode>,
    );
  })
  .catch((error) => {
    console.error("Erreur d'initialisation Keycloak :", error);
    rootElement.innerHTML =
      "<p>Impossible de contacter Keycloak. Vérifie que la stack Docker tourne (docker compose up).</p>";
  });
