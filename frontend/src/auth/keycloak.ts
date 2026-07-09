import Keycloak from "keycloak-js";
import { config } from "../config";

/**
 * Instance UNIQUE (singleton) du client Keycloak pour toute l'application.
 *
 * keycloak-js est la librairie officielle de l'équipe Keycloak : elle gère
 * elle-même tout le protocole OAuth2/OIDC "Authorization Code + PKCE" --
 * redirection vers la page de connexion hébergée par Keycloak, retour sur
 * notre application avec un code d'autorisation, échange de ce code contre
 * un jeton, et rafraîchissement du jeton. On ne réimplémente aucune de ces
 * étapes nous-mêmes : les faire à la main serait à la fois plus long et plus
 * risqué (erreur de sécurité facile sur ce genre de protocole).
 */
export const keycloak = new Keycloak({
  url: config.keycloakUrl,
  realm: config.keycloakRealm,
  clientId: config.keycloakClientId,
});
