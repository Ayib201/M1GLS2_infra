import { useEffect, useState } from "react";
import { ListChecks, ListTodo, Plus } from "lucide-react";
import { infraApi } from "../api/infraApiClient";
import { useApiCall } from "../hooks/useApiCall";
import { useAsyncAction } from "../hooks/useAsyncAction";
import { TacheItem } from "./TacheItem";
import { Alert, EmptyState, Spinner } from "./ui";

interface TachesPanelProps {
  token: string;
  projetId: string;
}

/**
 * Tâches d'un projet donné : liste + formulaire de création. Le
 * "projetId" vient toujours du parent (ProjetItem), jamais d'un état local
 * -- ce composant ne sait rien gérer en dehors de son projet.
 */
export function TachesPanel({ token, projetId }: TachesPanelProps) {
  const taches = useApiCall(() => infraApi.listerTaches(token, projetId));
  const [titre, setTitre] = useState("");
  const [description, setDescription] = useState("");
  const [dateEcheance, setDateEcheance] = useState("");

  const creation = useAsyncAction(
    (titreValeur: string, descriptionValeur: string, echeanceValeur: string) =>
      infraApi.creerTache(token, projetId, {
        titre: titreValeur,
        description: descriptionValeur || null,
        dateEcheance: echeanceValeur ? new Date(echeanceValeur).toISOString() : null,
      }),
  );

  useEffect(() => {
    taches.execute();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [projetId]);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!titre.trim()) return;

    const creee = await creation.execute(titre, description, dateEcheance);
    if (creee) {
      setTitre("");
      setDescription("");
      setDateEcheance("");
      taches.execute();
    }
  }

  const total = taches.data?.length ?? 0;

  return (
    <div className="taches-panel">
      <div className="subhead">
        <ListChecks size={15} aria-hidden="true" />
        Tâches {total > 0 && <span className="badge-count">{total}</span>}
      </div>

      {taches.error && <Alert message={taches.error} />}

      {taches.isLoading && (
        <div className="skeleton-list" aria-hidden="true">
          <div className="skeleton" style={{ height: 44 }} />
          <div className="skeleton" style={{ height: 44 }} />
        </div>
      )}

      {!taches.isLoading && total === 0 && (
        <EmptyState
          mini
          icon={<ListTodo size={20} />}
          title="Aucune tâche"
          description="Ajoutez la première tâche de ce projet ci-dessous."
        />
      )}

      {!taches.isLoading && total > 0 && (
        <ul className="taches-liste">
          {taches.data?.map((tache) => (
            <TacheItem
              key={tache.id}
              token={token}
              projetId={projetId}
              tache={tache}
              onChanged={taches.execute}
            />
          ))}
        </ul>
      )}

      <form onSubmit={handleSubmit} className="tache-form">
        <div className="form-grid">
          <div className="field field-full">
            <input
              className="input"
              type="text"
              placeholder="Titre de la tâche"
              aria-label="Titre de la tâche"
              value={titre}
              onChange={(e) => setTitre(e.target.value)}
              required
            />
          </div>
          <div className="field">
            <input
              className="input"
              type="text"
              placeholder="Description (optionnelle)"
              aria-label="Description de la tâche"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>
          <div className="field">
            <input
              className="input"
              type="date"
              aria-label="Date d'échéance"
              value={dateEcheance}
              onChange={(e) => setDateEcheance(e.target.value)}
            />
          </div>
        </div>
        <div className="form-actions">
          <button
            className="btn btn-secondary btn-sm"
            type="submit"
            disabled={creation.isLoading || !titre.trim()}
          >
            {creation.isLoading ? <Spinner /> : <Plus size={15} aria-hidden="true" />}
            {creation.isLoading ? "Ajout…" : "Ajouter une tâche"}
          </button>
        </div>
      </form>
      {creation.error && <Alert message={creation.error} />}
    </div>
  );
}
