import type { StatutTache } from "../types/api";

/**
 * Métadonnées d'affichage des statuts de tâche, centralisées ici pour rester
 * cohérentes partout (menu déroulant, badge, filtres). "tone" mappe vers les
 * classes CSS de badge définies dans App.css.
 */
export const STATUTS: Record<
  StatutTache,
  { libelle: string; tone: "todo" | "progress" | "done" }
> = {
  AFaire: { libelle: "À faire", tone: "todo" },
  EnCours: { libelle: "En cours", tone: "progress" },
  Terminee: { libelle: "Terminée", tone: "done" },
};

export const ORDRE_STATUTS: StatutTache[] = ["AFaire", "EnCours", "Terminee"];

/** Formate une date ISO en libellé court lisible (ex: "7 août 2026"). */
export function formaterDate(iso: string | null | undefined): string {
  if (!iso) return "";
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return "";
  return date.toLocaleDateString("fr-FR", {
    day: "numeric",
    month: "short",
    year: "numeric",
  });
}

/** Renvoie true si l'échéance est dépassée (utile pour signaler un retard). */
export function estEnRetard(iso: string | null | undefined): boolean {
  if (!iso) return false;
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return false;
  const aujourdhui = new Date();
  aujourdhui.setHours(0, 0, 0, 0);
  return date < aujourdhui;
}

/** Initiales d'un nom pour un avatar textuel. */
export function initiales(nom: string | undefined): string {
  if (!nom) return "?";
  const parts = nom.trim().split(/[\s._-]+/).filter(Boolean);
  if (parts.length === 0) return "?";
  if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
}
