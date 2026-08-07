import { useState } from "react";
import { ChevronRight, FolderKanban, Trash2 } from "lucide-react";
import { infraApi } from "../api/infraApiClient";
import { useAsyncAction } from "../hooks/useAsyncAction";
import type { Projet } from "../types/api";
import { TachesPanel } from "./TachesPanel";
import { Alert, Spinner } from "./ui";

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
    if (!window.confirm(`Supprimer le projet « ${projet.nom} » et tout son contenu ?`)) {
      return;
    }
    const ok = await suppression.execute();
    if (ok !== undefined) {
      onSupprime();
    }
  }

  const panelId = `projet-body-${projet.id}`;

  return (
    <li className={`projet-item${estDeplie ? " open" : ""}`}>
      <div className="projet-ligne">
        <button
          className="disclosure"
          onClick={() => setEstDeplie((v) => !v)}
          aria-expanded={estDeplie}
          aria-controls={panelId}
        >
          <span className="chevron" aria-hidden="true">
            <ChevronRight size={18} />
          </span>
          <span className="projet-icon" aria-hidden="true">
            <FolderKanban size={18} />
          </span>
          <span className="projet-meta">
            <span className="projet-title">
              <span className="projet-name">{projet.nom}</span>
            </span>
            {projet.description && (
              <span className="projet-description">{projet.description}</span>
            )}
          </span>
        </button>

        <div className="projet-actions">
          <button
            className="btn btn-danger-ghost btn-icon btn-sm"
            onClick={handleSupprimer}
            disabled={suppression.isLoading}
            aria-label={`Supprimer le projet ${projet.nom}`}
            title="Supprimer le projet"
          >
            {suppression.isLoading ? <Spinner /> : <Trash2 size={16} aria-hidden="true" />}
          </button>
        </div>
      </div>

      {suppression.error && (
        <div style={{ padding: "0 1.15rem 0.75rem" }}>
          <Alert message={suppression.error} />
        </div>
      )}

      {estDeplie && (
        <div className="projet-body" id={panelId}>
          <div className="divider" />
          <TachesPanel token={token} projetId={projet.id} />
        </div>
      )}
    </li>
  );
}
