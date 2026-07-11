import { useEffect, useState } from "react";
import { infraApi } from "../api/infraApiClient";
import { useApiCall } from "../hooks/useApiCall";
import { useAsyncAction } from "../hooks/useAsyncAction";

interface CommentairesPanelProps {
  token: string;
  projetId: string;
  tacheId: string;
}

/**
 * Dernier niveau de la hiérarchie métier : les commentaires d'une tâche.
 * Composant "feuille" -- ne connaît que son projetId/tacheId parents, reçus
 * en props (pas de state global, pas de contexte React : la hiérarchie de
 * composants reflète directement la hiérarchie des routes REST imbriquées).
 */
export function CommentairesPanel({ token, projetId, tacheId }: CommentairesPanelProps) {
  const commentaires = useApiCall(() => infraApi.listerCommentaires(token, projetId, tacheId));
  const [contenu, setContenu] = useState("");

  const creation = useAsyncAction((c: string) =>
    infraApi.creerCommentaire(token, projetId, tacheId, { contenu: c }),
  );
  const suppression = useAsyncAction((commentaireId: string) =>
    infraApi.supprimerCommentaire(token, projetId, tacheId, commentaireId),
  );

  useEffect(() => {
    commentaires.execute();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tacheId]);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!contenu.trim()) return;

    const cree = await creation.execute(contenu);
    if (cree) {
      setContenu("");
      commentaires.execute();
    }
  }

  async function handleSupprimer(commentaireId: string) {
    const ok = await suppression.execute(commentaireId);
    if (ok !== undefined) {
      commentaires.execute();
    }
  }

  return (
    <div className="commentaires-panel">
      <h5>Commentaires</h5>

      {commentaires.error && <p role="alert">{commentaires.error}</p>}

      <ul className="commentaires-liste">
        {commentaires.data?.map((commentaire) => (
          <li key={commentaire.id}>
            <span>{commentaire.contenu}</span>
            <button onClick={() => handleSupprimer(commentaire.id)} disabled={suppression.isLoading}>
              Supprimer
            </button>
          </li>
        ))}
        {commentaires.data?.length === 0 && <li className="vide">Aucun commentaire pour l'instant.</li>}
      </ul>

      <form onSubmit={handleSubmit} className="commentaire-form">
        <input
          type="text"
          placeholder="Ajouter un commentaire..."
          value={contenu}
          onChange={(e) => setContenu(e.target.value)}
        />
        <button type="submit" disabled={creation.isLoading}>
          {creation.isLoading ? "Envoi..." : "Envoyer"}
        </button>
      </form>
      {creation.error && <p role="alert">{creation.error}</p>}
    </div>
  );
}
