import { useEffect, useState } from "react";
import { keycloak } from "../auth/keycloak";

interface UseAuthResult {
  token: string;
  username: string | undefined;
  logout: () => void;
}

/**
 * Par le moment où ce hook est utilisé (dans App.tsx), `keycloak.init()`
 * (voir main.tsx) a déjà résolu avec succès -- on est donc nécessairement
 * authentifié (onLoad: "login-required" redirige automatiquement vers
 * Keycloak sinon, et App ne serait jamais rendu). Ce hook expose simplement
 * le jeton courant, et le tient à jour si keycloak-js le rafraîchit en
 * arrière-plan.
 */
export function useAuth(): UseAuthResult {
  const [token, setToken] = useState(keycloak.token ?? "");

  useEffect(() => {
    // Se déclenche après un rafraîchissement automatique du jeton -- on met
    // à jour notre state React pour que les prochains appels API utilisent
    // le nouveau jeton.
    keycloak.onAuthRefreshSuccess = () => setToken(keycloak.token ?? "");

    // Rafraîchit le jeton s'il expire dans moins de 30 secondes, plutôt que
    // de laisser une démo de plusieurs minutes tomber sur un 401 en plein
    // exposé devant le jury. Si le rafraîchissement échoue (session Keycloak
    // vraiment expirée), on relance une connexion.
    keycloak.onTokenExpired = () => {
      keycloak.updateToken(30).catch(() => keycloak.login());
    };
  }, []);

  return {
    token,
    username: keycloak.tokenParsed?.preferred_username as string | undefined,
    logout: () => keycloak.logout({ redirectUri: window.location.origin }),
  };
}
