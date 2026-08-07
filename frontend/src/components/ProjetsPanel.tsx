import { useEffect, useMemo, useState } from "react";
import { FolderKanban, FolderPlus, Plus, Search } from "lucide-react";
import { infraApi } from "../api/infraApiClient";
import { useApiCall } from "../hooks/useApiCall";
import { useAsyncAction } from "../hooks/useAsyncAction";
import { ProjetItem } from "./ProjetItem";
import { Alert, EmptyState, Spinner } from "./ui";

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
  const [recherche, setRecherche] = useState("");

  const creation = useAsyncAction((nomValeur: string, descriptionValeur: string) =>
    infraApi.creerProjet(token, { nom: nomValeur, description: descriptionValeur || null }),
  );

  useEffect(() => {
    projets.execute();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const projetsFiltres = useMemo(() => {
    const liste = projets.data ?? [];
    const q = recherche.trim().toLowerCase();
    if (!q) return liste;
    return liste.filter(
      (p) =>
        p.nom.toLowerCase().includes(q) ||
        (p.description?.toLowerCase().includes(q) ?? false),
    );
  }, [projets.data, recherche]);

  const total = projets.data?.length ?? 0;

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

  const aucunProjet = !projets.isLoading && total === 0;
  const rechercheSansResultat = !projets.isLoading && total > 0 && projetsFiltres.length === 0;

  return (
    <section className="projets-panel" aria-labelledby="projets-title">
      <div className="section-head">
        <div>
          <h2 id="projets-title">
            Mes projets{" "}
            {total > 0 && <span className="badge-count">{total}</span>}
          </h2>
          <p className="section-sub">
            Dépliez un projet pour gérer ses tâches et ses commentaires.
          </p>
        </div>

        {total > 0 && (
          <div className="search" role="search">
            <Search size={16} aria-hidden="true" />
            <input
              className="input"
              type="search"
              placeholder="Rechercher un projet…"
              aria-label="Rechercher un projet"
              value={recherche}
              onChange={(e) => setRecherche(e.target.value)}
            />
          </div>
        )}
      </div>

      {projets.error && <Alert message={projets.error} />}

      {projets.isLoading && (
        <div className="skeleton-list" aria-hidden="true">
          <div className="skeleton" />
          <div className="skeleton" />
          <div className="skeleton" />
        </div>
      )}

      {aucunProjet && (
        <EmptyState
          icon={<FolderPlus size={22} />}
          title="Aucun projet pour l'instant"
          description="Créez votre premier projet ci-dessous pour commencer à organiser vos tâches et vos échanges."
        />
      )}

      {rechercheSansResultat && (
        <EmptyState
          mini
          icon={<Search size={20} />}
          title="Aucun résultat"
          description={`Aucun projet ne correspond à « ${recherche} ».`}
        />
      )}

      {!projets.isLoading && projetsFiltres.length > 0 && (
        <ul className="projets-liste">
          {projetsFiltres.map((projet) => (
            <ProjetItem
              key={projet.id}
              token={token}
              projet={projet}
              onSupprime={projets.execute}
            />
          ))}
        </ul>
      )}

      <form onSubmit={handleSubmit} className="card create-form" style={{ marginTop: "1.5rem" }}>
        <div className="create-form-head">
          <FolderKanban size={17} aria-hidden="true" />
          Nouveau projet
        </div>

        <div className="form-grid">
          <div className="field">
            <label htmlFor="projet-nom">Nom du projet</label>
            <input
              id="projet-nom"
              className="input"
              type="text"
              placeholder="Ex. Refonte du site"
              value={nom}
              onChange={(e) => setNom(e.target.value)}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="projet-description">Description (optionnelle)</label>
            <input
              id="projet-description"
              className="input"
              type="text"
              placeholder="En une phrase, l'objectif du projet"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>
        </div>

        <div className="form-actions">
          <button className="btn btn-primary" type="submit" disabled={creation.isLoading || !nom.trim()}>
            {creation.isLoading ? <Spinner /> : <Plus size={16} aria-hidden="true" />}
            {creation.isLoading ? "Création…" : "Créer le projet"}
          </button>
        </div>

        {creation.error && <Alert message={creation.error} />}
      </form>
    </section>
  );
}
