import { useCallback } from "react";
import { useAuth0 } from "@auth0/auth0-react";

type FetchOptions = RequestInit & {
  headers?: Record<string, string>;
};

interface UseApiCallResult {
  apiCall: (
    url: string,
    options?: FetchOptions
  ) => Promise<Response>;
}

/**
 * Custom hook for making authenticated API calls
 * Automatically handles token acquisition and error handling
 */
export function useApiCall(): UseApiCallResult {
  const { getAccessTokenSilently } = useAuth0();

  const apiCall = useCallback(
    async (
      url: string,
      options: FetchOptions = {}
    ): Promise<Response> => {
      const token = await getAccessTokenSilently();

      const headers: Record<string, string> = {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      };

      if (options.headers) {
        Object.assign(headers, options.headers);
      }

      return fetch(url, {
        ...options,
        headers,
      });
    },
    [getAccessTokenSilently]
  );

  return { apiCall };
}
