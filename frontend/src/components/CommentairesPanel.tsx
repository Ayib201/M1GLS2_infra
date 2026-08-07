import { useEffect, useState } from "react";
import { MessagesSquare, Send, Trash2 } from "lucide-react";
import { infraApi } from "../api/infraApiClient";
import { useApiCall } from "../hooks/useApiCall";
import { useAsyncAction } from "../hooks/useAsyncAction";
import { Alert, Spinner } from "./ui";

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

  const total = commentaires.data?.length ?? 0;

  return (
    <div className="commentaires-panel">
      <div className="subhead">
        <MessagesSquare size={15} aria-hidden="true" />
        Commentaires {total > 0 && <span className="badge-count">{total}</span>}
      </div>

      {commentaires.error && <Alert message={commentaires.error} />}

      {!commentaires.isLoading && total === 0 && (
        <p className="empty-inline">Aucun commentaire pour l'instant — lancez la discussion.</p>
      )}

      {total > 0 && (
        <ul className="commentaires-liste">
          {commentaires.data?.map((commentaire) => (
            <li key={commentaire.id} className="commentaire-item">
              <span className="avatar" aria-hidden="true">
                <MessagesSquare size={13} />
              </span>
              <span className="commentaire-contenu">{commentaire.contenu}</span>
              <button
                className="btn btn-danger-ghost btn-icon btn-sm"
                onClick={() => handleSupprimer(commentaire.id)}
                disabled={suppression.isLoading}
                aria-label="Supprimer le commentaire"
                title="Supprimer le commentaire"
              >
                <Trash2 size={14} aria-hidden="true" />
              </button>
            </li>
          ))}
        </ul>
      )}

      <form onSubmit={handleSubmit} className="inline-form">
        <input
          className="input"
          type="text"
          placeholder="Ajouter un commentaire…"
          aria-label="Ajouter un commentaire"
          value={contenu}
          onChange={(e) => setContenu(e.target.value)}
        />
        <button
          className="btn btn-primary btn-sm"
          type="submit"
          disabled={creation.isLoading || !contenu.trim()}
        >
          {creation.isLoading ? <Spinner /> : <Send size={14} aria-hidden="true" />}
          {creation.isLoading ? "Envoi…" : "Envoyer"}
        </button>
      </form>
      {creation.error && <Alert message={creation.error} />}
    </div>
  );
}
