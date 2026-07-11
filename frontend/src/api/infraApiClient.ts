import { config } from "../config";
import type {
  ApiErrorResponse,
  Commentaire,
  CreerCommentaireRequest,
  CreerProjetRequest,
  CreerTacheRequest,
  MettreAJourTacheRequest,
  Projet,
  Tache,
} from "../types/api";

/**
 * Fonction générique privée : centralise "comment on appelle l'API à
 * travers Kong" (URL de base, en-tête Authorization, sérialisation JSON du
 * corps, gestion des erreurs HTTP). Les fonctions exportées plus bas ne font
 * que préciser le chemin, la méthode et le type de retour attendu -- on ne
 * répète jamais la logique fetch/erreur.
 */
async function callThroughKong<T>(
  path: string,
  token: string,
  method: "GET" | "POST" | "PUT" | "DELETE" = "GET",
  body?: unknown,
): Promise<T> {
  const response = await fetch(`${config.kongBaseUrl}${path}`, {
    method,
    headers: {
      Authorization: `Bearer ${token}`,
      ...(body !== undefined ? { "Content-Type": "application/json" } : {}),
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!response.ok) {
    const errorBody = (await response.json().catch(() => null)) as ApiErrorResponse | null;
    throw new Error(errorBody?.error ?? `Erreur HTTP ${response.status} sur ${path}`);
  }

  // 204 No Content (suppressions) n'a pas de corps JSON à parser.
  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

// Toujours passer par Kong (jamais directement par l'API .NET) : c'est le
// point d'entrée unique décidé dès le début de ce projet. Une seule route
// Kong ("/api/v1/projets", voir kong/kong.yml) couvre tout le domaine
// métier, y compris les sous-ressources imbriquées ci-dessous.
export const infraApi = {
  // --- Projets ---
  listerProjets: (token: string) => callThroughKong<Projet[]>("/api/v1/projets", token),

  creerProjet: (token: string, requete: CreerProjetRequest) =>
    callThroughKong<Projet>("/api/v1/projets", token, "POST", requete),

  supprimerProjet: (token: string, projetId: string) =>
    callThroughKong<void>(`/api/v1/projets/${projetId}`, token, "DELETE"),

  // --- Tâches (imbriquées sous un projet) ---
  listerTaches: (token: string, projetId: string) =>
    callThroughKong<Tache[]>(`/api/v1/projets/${projetId}/taches`, token),

  creerTache: (token: string, projetId: string, requete: CreerTacheRequest) =>
    callThroughKong<Tache>(`/api/v1/projets/${projetId}/taches`, token, "POST", requete),

  mettreAJourTache: (
    token: string,
    projetId: string,
    tacheId: string,
    requete: MettreAJourTacheRequest,
  ) =>
    callThroughKong<Tache>(
      `/api/v1/projets/${projetId}/taches/${tacheId}`,
      token,
      "PUT",
      requete,
    ),

  supprimerTache: (token: string, projetId: string, tacheId: string) =>
    callThroughKong<void>(`/api/v1/projets/${projetId}/taches/${tacheId}`, token, "DELETE"),

  // --- Commentaires (imbriqués sous une tâche) ---
  listerCommentaires: (token: string, projetId: string, tacheId: string) =>
    callThroughKong<Commentaire[]>(
      `/api/v1/projets/${projetId}/taches/${tacheId}/commentaires`,
      token,
    ),

  creerCommentaire: (
    token: string,
    projetId: string,
    tacheId: string,
    requete: CreerCommentaireRequest,
  ) =>
    callThroughKong<Commentaire>(
      `/api/v1/projets/${projetId}/taches/${tacheId}/commentaires`,
      token,
      "POST",
      requete,
    ),

  supprimerCommentaire: (token: string, projetId: string, tacheId: string, commentaireId: string) =>
    callThroughKong<void>(
      `/api/v1/projets/${projetId}/taches/${tacheId}/commentaires/${commentaireId}`,
      token,
      "DELETE",
    ),
};
