import "./App.css";
import { FolderKanban, LogOut, ListChecks, MessagesSquare, Sparkles } from "lucide-react";
import { ProjetsPanel } from "./components/ProjetsPanel";
import { useAuth } from "./hooks/useAuth";
import { initiales } from "./lib/format";

/**
 * Composant racine : orchestre les hooks (état) et les composants
 * (affichage). Ce composant n'est rendu QUE si Keycloak a déjà confirmé
 * l'authentification (voir main.tsx) -- pas besoin de gérer un écran de
 * connexion ici, la redirection Keycloak s'en est déjà chargée en amont.
 *
 * Le profil Utilisateur (PostgreSQL) n'a plus besoin d'être créé
 * explicitement depuis le front : il se provisionne automatiquement dès le
 * premier appel métier authentifié (voir Services/UtilisateurCourantService.cs).
 */
export default function App() {
  const auth = useAuth();
  const username = auth.username ?? "utilisateur";

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand">
          <span className="brand-mark" aria-hidden="true">
            <FolderKanban size={19} />
          </span>
          <span className="brand-name">
            Atelier <span>· espace projets</span>
          </span>
        </div>

        <div className="topbar-spacer" />

        <div className="user-chip">
          <div className="user-meta">
            <span className="user-label">Connecté</span>
            <span className="user-name">{username}</span>
          </div>
          <span className="avatar" aria-hidden="true">
            {initiales(auth.username)}
          </span>
        </div>

        <button className="btn btn-secondary btn-sm" onClick={auth.logout}>
          <LogOut size={15} aria-hidden="true" />
          Se déconnecter
        </button>
      </header>

      <main className="app-main">
        <section className="hero" aria-labelledby="hero-title">
          <span className="hero-eyebrow">
            <Sparkles size={13} aria-hidden="true" />
            Espace de travail sécurisé
          </span>
          <h1 id="hero-title">
            Organisez vos projets, du premier brief à la dernière tâche livrée.
          </h1>
          <p>
            Atelier centralise vos projets, leurs tâches et les échanges de votre
            équipe. Créez un projet, découpez-le en tâches suivies par statut, et
            gardez la discussion au plus près de chaque action — le tout derrière
            une authentification gérée par Keycloak.
          </p>

          <div className="hero-features">
            <span className="hero-feature">
              <FolderKanban size={16} aria-hidden="true" />
              Projets structurés
            </span>
            <span className="hero-feature">
              <ListChecks size={16} aria-hidden="true" />
              Tâches suivies par statut
            </span>
            <span className="hero-feature">
              <MessagesSquare size={16} aria-hidden="true" />
              Commentaires contextuels
            </span>
          </div>
        </section>

        <ProjetsPanel token={auth.token} />
      </main>
    </div>
  );
}
