import { infraApi } from "./api/infraApiClient";
import { ServiceCard } from "./components/ServiceCard";
import { useApiCall } from "./hooks/useApiCall";
import { useAuth } from "./hooks/useAuth";

/**
 * Composant racine : orchestre les hooks (état) et les composants
 * (affichage). Ce composant n'est rendu QUE si Keycloak a déjà confirmé
 * l'authentification (voir main.tsx) -- pas besoin de gérer un écran de
 * connexion ici, la redirection Keycloak s'en est déjà chargée en amont.
 */
export default function App() {
  const auth = useAuth();

  const serviceA = useApiCall(() => infraApi.getServiceA(auth.token));
  const serviceB = useApiCall(() => infraApi.getServiceB(auth.token));
  const profil = useApiCall(() => infraApi.creerProfil(auth.token));

  return (
    <main>
      <header>
        <h1>Démo Infra — Kong / Vault / Keycloak / PostgreSQL</h1>
        <p>Connecté en tant que {auth.username ?? "utilisateur inconnu"}</p>
        <button onClick={auth.logout}>Se déconnecter</button>
      </header>

      <ServiceCard
        title="Service A (secret Vault)"
        buttonLabel="Appeler /service-a"
        onTrigger={serviceA.execute}
        isLoading={serviceA.isLoading}
        error={serviceA.error}
        result={serviceA.data}
      />

      <ServiceCard
        title="Service B"
        buttonLabel="Appeler /service-b"
        onTrigger={serviceB.execute}
        isLoading={serviceB.isLoading}
        error={serviceB.error}
        result={serviceB.data}
      />

      <ServiceCard
        title="Créer mon profil (PostgreSQL, protégé par Keycloak)"
        buttonLabel="Appeler /api/v1/profils/creer"
        onTrigger={profil.execute}
        isLoading={profil.isLoading}
        error={profil.error}
        result={profil.data}
      />
    </main>
  );
}
