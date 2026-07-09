import { config } from "../config";
import type {
  ApiErrorResponse,
  ProfilResponse,
  ServiceAResponse,
  ServiceBResponse,
} from "../types/api";

/**
 * Fonction générique privée : centralise "comment on appelle l'API à
 * travers Kong" (URL de base, en-tête Authorization, gestion des erreurs
 * HTTP). Les fonctions exportées plus bas ne font que préciser le chemin et
 * le type de retour attendu -- on ne répète jamais la logique fetch/erreur.
 */
async function callThroughKong<T>(
  path: string,
  token: string,
  method: "GET" | "POST" = "GET",
): Promise<T> {
  const response = await fetch(`${config.kongBaseUrl}${path}`, {
    method,
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    const errorBody = (await response.json().catch(() => null)) as ApiErrorResponse | null;
    throw new Error(errorBody?.error ?? `Erreur HTTP ${response.status} sur ${path}`);
  }

  return (await response.json()) as T;
}

// Toujours passer par Kong (jamais directement par l'API .NET) : c'est le
// point d'entrée unique décidé dès le début de ce projet.
export const infraApi = {
  getServiceA: (token: string) => callThroughKong<ServiceAResponse>("/service-a", token),
  getServiceB: (token: string) => callThroughKong<ServiceBResponse>("/service-b", token),
  creerProfil: (token: string) =>
    callThroughKong<ProfilResponse>("/api/v1/profils/creer", token, "POST"),
};
