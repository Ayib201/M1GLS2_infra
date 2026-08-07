import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App";
import "./App.css";
import { ErrorScreen, LoadingScreen } from "./components/StatusScreens";
import { keycloak } from "./auth/keycloak";

const rootElement = document.getElementById("root");
if (!rootElement) {
  throw new Error("Élément #root introuvable dans index.html.");
}

const root = createRoot(rootElement);

// Écran de chargement soigné pendant que keycloak-js tranche la question de
// l'authentification (au lieu d'un écran blanc).
root.render(
  <StrictMode>
    <LoadingScreen />
  </StrictMode>,
);

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

    root.render(
      <StrictMode>
        <App />
      </StrictMode>,
    );
  })
  .catch((error) => {
    console.error("Erreur d'initialisation Keycloak :", error);
    root.render(
      <StrictMode>
        <ErrorScreen />
      </StrictMode>,
    );
  });
