import { useAuth0 } from "@auth0/auth0-react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ApiError, validationMessages } from "../api/ApiError";
import type {
	AnalyzeSurveyCommand,
	CreateSurveyCommand,
	UserSurveyModel,
} from "../types";
import { useApiClient } from "./useApiClient";

export const surveyKeys = {
	all: ["surveys"] as const,
	list: (userId: string | undefined) => ["surveys", userId, "list"] as const,
	detail: (userId: string | undefined, id: number | null) =>
		["surveys", userId, "detail", id] as const,
};

export function useUserSurveys(enabled: boolean) {
	const client = useApiClient();
	const { user, isAuthenticated } = useAuth0();
	return useQuery({
		queryKey: surveyKeys.list(user?.sub),
		enabled: enabled && isAuthenticated,
		queryFn: async ({ signal }) => {
			const { data, response } = await client.GET("/api/survey/user", {
				signal,
			});
			if (!response.ok || !data)
				throw new ApiError("Failed to fetch surveys", response.status);
			return data;
		},
	});
}

export function useCreateSurvey() {
	const client = useApiClient();
	const cache = useQueryClient();
	const { user } = useAuth0();
	return useMutation({
		mutationFn: async (body: CreateSurveyCommand) => {
			const { data, error, response } = await client.POST("/api/survey", {
				body,
			});
			if (response.status !== 201 || !data) {
				throw new ApiError(
					"Please try again or create an issue on GitHub",
					response.status,
					response.status === 422 ? validationMessages(error) : [],
				);
			}
			return data;
		},
		onSuccess: async (survey) => {
			const key = surveyKeys.detail(user?.sub, survey.id);
			await cache.cancelQueries({ queryKey: key });
			cache.setQueryData(key, survey);
			await cache.invalidateQueries({ queryKey: surveyKeys.list(user?.sub) });
		},
	});
}

export function useAnalyzeSurvey() {
	const client = useApiClient();
	return useMutation({
		mutationFn: async (body: AnalyzeSurveyCommand) => {
			const { data, error, response } = await client.POST(
				"/api/survey/analyze",
				{ body },
			);
			if (!response.ok || !data) {
				throw new ApiError(
					"Survey analysis is temporarily unavailable. Please try again.",
					response.status,
					response.status === 422 ? validationMessages(error) : [],
				);
			}
			return data;
		},
	});
}

export function useDeleteSurvey() {
	const client = useApiClient();
	const cache = useQueryClient();
	const { user } = useAuth0();
	return useMutation({
		mutationFn: async (id: number) => {
			const { response } = await client.DELETE("/api/survey/{id}", {
				params: { path: { id } },
			});
			if (!response.ok)
				throw new ApiError("Failed to delete survey", response.status);
			return id;
		},
		onSuccess: async (id) => {
			await Promise.all([
				cache.cancelQueries({ queryKey: surveyKeys.detail(user?.sub, id) }),
				cache.cancelQueries({ queryKey: surveyKeys.list(user?.sub) }),
			]);
			// An active detail view must clear immediately, without refetching the deleted record.
			cache.setQueryData(surveyKeys.detail(user?.sub, id), null);
			cache.setQueryData<UserSurveyModel[]>(
				surveyKeys.list(user?.sub),
				(surveys) => surveys?.filter((survey) => survey.id !== id),
			);
			await cache.invalidateQueries({ queryKey: surveyKeys.list(user?.sub) });
		},
	});
}
