import { useEffect, useState } from "react";
import { infraApi } from "../api/infraApiClient";
import { useApiCall } from "../hooks/useApiCall";
import { useAsyncAction } from "../hooks/useAsyncAction";
import { ProjetItem } from "./ProjetItem";

interface ProjetsPanelProps {
  token: string;
}

/**
 * Racine de l'écran métier : liste des projets de l'utilisateur connecté +
 * formulaire de création. C'est le seul composant qui charge des données au
 * montage (useEffect) -- tout le reste (tâches, commentaires) ne se charge
 * qu'à la demande, quand l'utilisateur déplie un élément.
 */
export function ProjetsPanel({ token }: ProjetsPanelProps) {
  const projets = useApiCall(() => infraApi.listerProjets(token));
  const [nom, setNom] = useState("");
  const [description, setDescription] = useState("");

  const creation = useAsyncAction((nomValeur: string, descriptionValeur: string) =>
    infraApi.creerProjet(token, { nom: nomValeur, description: descriptionValeur || null }),
  );

  useEffect(() => {
    projets.execute();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!nom.trim()) return;

    const cree = await creation.execute(nom, description);
    if (cree) {
      setNom("");
      setDescription("");
      projets.execute();
    }
  }

  return (
    <section className="projets-panel">
      <h2>Mes projets</h2>

      {projets.isLoading && <p>Chargement des projets...</p>}
      {projets.error && <p role="alert">{projets.error}</p>}

      <ul className="projets-liste">
        {projets.data?.map((projet) => (
          <ProjetItem key={projet.id} token={token} projet={projet} onSupprime={projets.execute} />
        ))}
        {projets.data?.length === 0 && <li className="vide">Aucun projet pour l'instant -- crée le premier ci-dessous.</li>}
      </ul>

      <form onSubmit={handleSubmit} className="projet-form">
        <h3>Nouveau projet</h3>
        <input
          type="text"
          placeholder="Nom du projet"
          value={nom}
          onChange={(e) => setNom(e.target.value)}
        />
        <input
          type="text"
          placeholder="Description (optionnelle)"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
        <button type="submit" disabled={creation.isLoading}>
          {creation.isLoading ? "Création..." : "Créer le projet"}
        </button>
      </form>
      {creation.error && <p role="alert">{creation.error}</p>}
    </section>
  );
}
