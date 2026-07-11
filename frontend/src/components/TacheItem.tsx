import { useState } from "react";
import { infraApi } from "../api/infraApiClient";
import { useAsyncAction } from "../hooks/useAsyncAction";
import type { StatutTache, Tache } from "../types/api";
import { CommentairesPanel } from "./CommentairesPanel";

interface TacheItemProps {
  token: string;
  projetId: string;
  tache: Tache;
  onChanged: () => void;
}

const LIBELLES_STATUT: Record<StatutTache, string> = {
  AFaire: "À faire",
  EnCours: "En cours",
  Terminee: "Terminée",
};

/**
 * Une tâche : son statut peut être changé directement depuis la liste (menu
 * déroulant), et ses commentaires ne s'affichent qu'une fois dépliée --
 * évite de charger tous les commentaires de toutes les tâches d'un coup.
 */
export function TacheItem({ token, projetId, tache, onChanged }: TacheItemProps) {
  const [estDepliee, setEstDepliee] = useState(false);

  const changementStatut = useAsyncAction((nouveauStatut: StatutTache) =>
    infraApi.mettreAJourTache(token, projetId, tache.id, {
      titre: tache.titre,
      description: tache.description,
      dateEcheance: tache.dateEcheance,
      statut: nouveauStatut,
    }),
  );
  const suppression = useAsyncAction(() => infraApi.supprimerTache(token, projetId, tache.id));

  async function handleChangementStatut(event: React.ChangeEvent<HTMLSelectElement>) {
    const majTache = await changementStatut.execute(event.target.value as StatutTache);
    if (majTache) {
      onChanged();
    }
  }

  async function handleSupprimer() {
    const ok = await suppression.execute();
    if (ok !== undefined) {
      onChanged();
    }
  }

  return (
    <li className="tache-item">
      <div className="tache-ligne">
        <button className="lien-depliant" onClick={() => setEstDepliee((v) => !v)}>
          {estDepliee ? "▾" : "▸"} {tache.titre}
        </button>

        <select value={tache.statut} onChange={handleChangementStatut} disabled={changementStatut.isLoading}>
          {(Object.keys(LIBELLES_STATUT) as StatutTache[]).map((statut) => (
            <option key={statut} value={statut}>
              {LIBELLES_STATUT[statut]}
            </option>
          ))}
        </select>

        <button onClick={handleSupprimer} disabled={suppression.isLoading}>
          Supprimer
        </button>
      </div>

      {tache.description && <p className="tache-description">{tache.description}</p>}
      {changementStatut.error && <p role="alert">{changementStatut.error}</p>}

      {estDepliee && <CommentairesPanel token={token} projetId={projetId} tacheId={tache.id} />}
    </li>
  );
}
