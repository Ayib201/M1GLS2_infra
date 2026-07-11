import { useCallback, useState } from "react";

interface UseAsyncActionResult<Args extends unknown[], T> {
  execute: (...args: Args) => Promise<T | undefined>;
  isLoading: boolean;
  error: string | null;
}

/**
 * Complément de useApiCall : là où useApiCall gère un appel SANS argument
 * (ex: "liste mes projets"), ce hook gère une action qui reçoit des
 * arguments au moment de son déclenchement (ex: "crée ce projet précis",
 * "supprime cette tâche précise"). Même trio chargement/erreur/résultat,
 * même logique try/catch/finally -- pas de duplication entre les formulaires
 * de création et les boutons de suppression du domaine métier.
 */
export function useAsyncAction<Args extends unknown[], T>(
  action: (...args: Args) => Promise<T>,
): UseAsyncActionResult<Args, T> {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const execute = useCallback(
    async (...args: Args) => {
      setIsLoading(true);
      setError(null);

      try {
        return await action(...args);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Erreur inconnue.");
        return undefined;
      } finally {
        setIsLoading(false);
      }
      // eslint-disable-next-line react-hooks/exhaustive-deps
    },
    [action],
  );

  return { execute, isLoading, error };
}
