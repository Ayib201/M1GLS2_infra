import { useEffect, useState } from "react";
import { infraApi } from "../api/infraApiClient";
import { useApiCall } from "../hooks/useApiCall";
import { useAsyncAction } from "../hooks/useAsyncAction";
import { TacheItem } from "./TacheItem";

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

  return (
    <div className="taches-panel">
      <h4>Tâches</h4>

      {taches.error && <p role="alert">{taches.error}</p>}

      <ul className="taches-liste">
        {taches.data?.map((tache) => (
          <TacheItem key={tache.id} token={token} projetId={projetId} tache={tache} onChanged={taches.execute} />
        ))}
        {taches.data?.length === 0 && <li className="vide">Aucune tâche pour l'instant.</li>}
      </ul>

      <form onSubmit={handleSubmit} className="tache-form">
        <input
          type="text"
          placeholder="Titre de la tâche"
          value={titre}
          onChange={(e) => setTitre(e.target.value)}
        />
        <input
          type="text"
          placeholder="Description (optionnelle)"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
        <input
          type="date"
          value={dateEcheance}
          onChange={(e) => setDateEcheance(e.target.value)}
        />
        <button type="submit" disabled={creation.isLoading}>
          {creation.isLoading ? "Création..." : "Ajouter une tâche"}
        </button>
      </form>
      {creation.error && <p role="alert">{creation.error}</p>}
    </div>
  );
}
