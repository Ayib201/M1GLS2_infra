interface ServiceCardProps {
  title: string;
  buttonLabel: string;
  onTrigger: () => void;
  isLoading: boolean;
  error: string | null;
  result: unknown;
}

/**
 * Carte réutilisable pour les 3 démos (Service A, Service B, création de
 * profil) : un bouton, un état de chargement, une erreur éventuelle, et le
 * JSON brut renvoyé par l'API. Réutiliser CE composant 3 fois (voir App.tsx)
 * évite de copier-coller la même structure JSX trois fois.
 */
export function ServiceCard({
  title,
  buttonLabel,
  onTrigger,
  isLoading,
  error,
  result,
}: ServiceCardProps) {
  return (
    <section>
      <h3>{title}</h3>

      <button onClick={onTrigger} disabled={isLoading}>
        {isLoading ? "Appel en cours..." : buttonLabel}
      </button>

      {error && <p role="alert">{error}</p>}

      {result != null && <pre>{JSON.stringify(result, null, 2)}</pre>}
    </section>
  );
}
