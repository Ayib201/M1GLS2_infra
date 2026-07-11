import "./App.css";
import { ProjetsPanel } from "./components/ProjetsPanel";
import { useAuth } from "./hooks/useAuth";

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

  return (
    <main>
      <header>
        <h1>Gestion de projets — Kong / Vault / Keycloak / PostgreSQL</h1>
        <p>Connecté en tant que {auth.username ?? "utilisateur inconnu"}</p>
        <button onClick={auth.logout}>Se déconnecter</button>
      </header>

      <ProjetsPanel token={auth.token} />
    </main>
  );
}
