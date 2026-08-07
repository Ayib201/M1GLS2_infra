import type { ReactNode } from "react";
import { AlertCircle } from "lucide-react";

/**
 * Petits composants d'UI partagés pour garder un rendu cohérent des états
 * (erreur, vide) sur tous les panneaux, sans dupliquer le markup.
 */

export function Alert({ message }: { message: string }) {
  return (
    <p className="alert" role="alert">
      <AlertCircle size={16} aria-hidden="true" />
      <span>{message}</span>
    </p>
  );
}

interface EmptyStateProps {
  icon: ReactNode;
  title: string;
  description?: string;
  action?: ReactNode;
  mini?: boolean;
}

export function EmptyState({ icon, title, description, action, mini }: EmptyStateProps) {
  return (
    <div className={`empty-state${mini ? " mini" : ""}`}>
      <div className="empty-icon" aria-hidden="true">
        {icon}
      </div>
      <h3>{title}</h3>
      {description && <p>{description}</p>}
      {action}
    </div>
  );
}

/** Spinner à afficher dans un bouton pendant une action asynchrone. */
export function Spinner() {
  return <span className="spinner" aria-hidden="true" />;
}
