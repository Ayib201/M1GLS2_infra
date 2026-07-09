import { useCallback, useState } from "react";

interface UseApiCallResult<T> {
  data: T | null;
  isLoading: boolean;
  error: string | null;
  execute: () => Promise<void>;
}

/**
 * Hook générique qui encapsule le trio "chargement / erreur / résultat"
 * commun à n'importe quel appel API. Sans lui, on dupliquerait ces trois
 * `useState` (et la logique try/catch/finally) dans Service A, Service B ET
 * la création de profil -- trois copies du même code. Le type générique <T>
 * garde le typage précis (ServiceAResponse, ServiceBResponse, ProfilResponse)
 * pour chaque appel, malgré la logique partagée.
 */
export function useApiCall<T>(apiFunction: () => Promise<T>): UseApiCallResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const execute = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const result = await apiFunction();
      setData(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Erreur inconnue.");
    } finally {
      setIsLoading(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [apiFunction]);

  return { data, isLoading, error, execute };
}
