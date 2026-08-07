import { FolderKanban, ServerCrash } from "lucide-react";

/**
 * Écrans plein-écran affichés AVANT que l'application métier ne soit montée :
 * pendant que Keycloak vérifie la session, ou si Keycloak est injoignable.
 * Ils remplacent l'ancien message brut pour offrir un premier contact soigné.
 */

export function LoadingScreen() {
  return (
    <div className="fullscreen">
      <div className="fullscreen-card">
        <div className="big-spinner" aria-hidden="true" />
        <h1>Connexion en cours…</h1>
        <p>Vérification de votre session auprès de Keycloak.</p>
      </div>
    </div>
  );
}

export function ErrorScreen() {
  return (
    <div className="fullscreen">
      <div className="fullscreen-card">
        <div className="error-mark" aria-hidden="true">
          <ServerCrash size={24} />
        </div>
        <h1>Service d&apos;authentification injoignable</h1>
        <p>
          Impossible de contacter Keycloak pour le moment. Vérifiez que la stack
          (Kong, Keycloak, PostgreSQL) est bien démarrée, puis réessayez.
        </p>
        <button
          className="btn btn-primary btn-block"
          style={{ marginTop: "1.25rem" }}
          onClick={() => window.location.reload()}
        >
          Réessayer
        </button>
        <p style={{ marginTop: "1.25rem", display: "flex", alignItems: "center", justifyContent: "center", gap: "0.4rem" }}>
          <FolderKanban size={14} aria-hidden="true" />
          Atelier · espace projets
        </p>
      </div>
    </div>
  );
}
