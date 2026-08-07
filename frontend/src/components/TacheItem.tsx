import { useState } from "react";
import { CalendarClock, ChevronRight, Trash2 } from "lucide-react";
import { infraApi } from "../api/infraApiClient";
import { useAsyncAction } from "../hooks/useAsyncAction";
import type { StatutTache, Tache } from "../types/api";
import { ORDRE_STATUTS, STATUTS, estEnRetard, formaterDate } from "../lib/format";
import { CommentairesPanel } from "./CommentairesPanel";
import { Alert, Spinner } from "./ui";

interface TacheItemProps {
  token: string;
  projetId: string;
  tache: Tache;
  onChanged: () => void;
}

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

  const estTerminee = tache.statut === "Terminee";
  const enRetard = !estTerminee && estEnRetard(tache.dateEcheance);
  const panelId = `tache-body-${tache.id}`;

  return (
    <li className={`tache-item${estDepliee ? " open" : ""}`}>
      <div className="tache-ligne">
        <button
          className="disclosure"
          onClick={() => setEstDepliee((v) => !v)}
          aria-expanded={estDepliee}
          aria-controls={panelId}
        >
          <span className="chevron" aria-hidden="true">
            <ChevronRight size={16} />
          </span>
          <span className={`tache-title${estTerminee ? " done" : ""}`}>{tache.titre}</span>
        </button>

        <div className="tache-controls">
          <span className={`badge badge-${STATUTS[tache.statut].tone}`}>
            {STATUTS[tache.statut].libelle}
          </span>

          <select
            className="select"
            value={tache.statut}
            onChange={handleChangementStatut}
            disabled={changementStatut.isLoading}
            aria-label={`Statut de la tâche ${tache.titre}`}
          >
            {ORDRE_STATUTS.map((statut) => (
              <option key={statut} value={statut}>
                {STATUTS[statut].libelle}
              </option>
            ))}
          </select>

          <button
            className="btn btn-danger-ghost btn-icon btn-sm"
            onClick={handleSupprimer}
            disabled={suppression.isLoading}
            aria-label={`Supprimer la tâche ${tache.titre}`}
            title="Supprimer la tâche"
          >
            {suppression.isLoading ? <Spinner /> : <Trash2 size={15} aria-hidden="true" />}
          </button>
        </div>
      </div>

      {changementStatut.error && (
        <div style={{ padding: "0 0.75rem 0.5rem 2.05rem" }}>
          <Alert message={changementStatut.error} />
        </div>
      )}
      {suppression.error && (
        <div style={{ padding: "0 0.75rem 0.5rem 2.05rem" }}>
          <Alert message={suppression.error} />
        </div>
      )}

      {estDepliee && (
        <div className="tache-body" id={panelId}>
          {tache.description && <p className="tache-description">{tache.description}</p>}
          {tache.dateEcheance && (
            <p className={`badge-due${enRetard ? " overdue" : ""}`} style={{ marginBottom: "0.6rem" }}>
              <CalendarClock size={14} aria-hidden="true" />
              Échéance : {formaterDate(tache.dateEcheance)}
              {enRetard && " · en retard"}
            </p>
          )}
          <CommentairesPanel token={token} projetId={projetId} tacheId={tache.id} />
        </div>
      )}
    </li>
  );
}
