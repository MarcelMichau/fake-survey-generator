import { ApiError } from "../api/ApiError";
import {
	useRegisteredSurveyQuery,
	useSurveySession,
} from "../api/SurveySession";
import { useApiClient } from "./useApiClient";
import { surveyKeys } from "./useSurveys";

export function useSurveyFetch(surveyId: number | null) {
	const client = useApiClient();
	const session = useSurveySession();
	const enabled = surveyId !== null && surveyId > 0;
	const query = useRegisteredSurveyQuery({
		queryKey: surveyKeys.detail(session.userId, surveyId),
		enabled,
		queryFn: async ({ signal }) => {
			if (surveyId === null) throw new Error("A survey ID is required");
			const { data, response } = await client.GET("/api/survey/{id}", {
				params: { path: { id: surveyId } },
				signal,
			});
			if (!response.ok || !data) {
				throw new ApiError(
					response.status === 404
						? "Looks like that survey does not exist"
						: "Something did not go as planned",
					response.status,
				);
			}
			return data;
		},
	});
	return {
		survey:
			session.isReady && enabled && !query.isError
				? (query.data ?? null)
				: null,
		loading: query.isFetching,
		error: query.error?.message ?? "",
		refetch: query.refetch,
	};
}
