import { useState } from "react";
import { infraApi } from "../api/infraApiClient";
import { useAsyncAction } from "../hooks/useAsyncAction";
import type { Projet } from "../types/api";
import { TachesPanel } from "./TachesPanel";

interface ProjetItemProps {
  token: string;
  projet: Projet;
  onSupprime: () => void;
}

/**
 * Un projet dans la liste : dépliable pour révéler ses tâches (TachesPanel),
 * qui ne sont chargées qu'à ce moment-là (pas au chargement initial de la
 * liste de projets) -- évite N+1 appels API inutiles si l'utilisateur ne
 * consulte jamais le détail d'un projet donné.
 */
export function ProjetItem({ token, projet, onSupprime }: ProjetItemProps) {
  const [estDeplie, setEstDeplie] = useState(false);
  const suppression = useAsyncAction(() => infraApi.supprimerProjet(token, projet.id));

  async function handleSupprimer() {
    const ok = await suppression.execute();
    if (ok !== undefined) {
      onSupprime();
    }
  }

  return (
    <li className="projet-item">
      <div className="projet-ligne">
        <button className="lien-depliant" onClick={() => setEstDeplie((v) => !v)}>
          {estDeplie ? "▾" : "▸"} <strong>{projet.nom}</strong>
        </button>
        <button onClick={handleSupprimer} disabled={suppression.isLoading}>
          Supprimer le projet
        </button>
      </div>

      {projet.description && <p className="projet-description">{projet.description}</p>}
      {suppression.error && <p role="alert">{suppression.error}</p>}

      {estDeplie && <TachesPanel token={token} projetId={projet.id} />}
    </li>
  );
}
