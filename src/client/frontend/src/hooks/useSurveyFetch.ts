import { useState, useEffect } from "react";
import { useApiCall } from "./useApiCall";
import type * as Types from "../types";

interface UseSurveyFetchResult {
  survey: Types.SurveyModel | null;
  loading: boolean;
  error: string;
}

/**
 * Custom hook for fetching a single survey by ID
 * Handles loading state and error management
 */
export function useSurveyFetch(surveyId: number | null): UseSurveyFetchResult {
  const { apiCall } = useApiCall();
  const [survey, setSurvey] = useState<Types.SurveyModel | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (surveyId === null || surveyId === 0) {
      setSurvey(null);
      return;
    }

    const fetchSurvey = async () => {
      setLoading(true);
      setError("");

      try {
        const url = `api/survey/${surveyId}`;
        const response = await apiCall(url);

        if (response.status === 404) {
          setError("Looks like that survey does not exist");
          setSurvey(null);
          return;
        }

        if (response.status !== 200) {
          setError("Something did not go as planned");
          setSurvey(null);
          return;
        }

        const data: Types.SurveyModel = await response.json();
        setSurvey(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : "An error occurred");
        setSurvey(null);
      } finally {
        setLoading(false);
      }
    };

    fetchSurvey();
  }, [surveyId, apiCall]);

  return { survey, loading, error };
}
