import { useQueryClient } from "@tanstack/react-query";
import { ApiError, validationMessages } from "../api/ApiError";
import {
	useRegisteredSurveyMutation,
	useRegisteredSurveyQuery,
	useSurveySession,
} from "../api/SurveySession";
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
	const { userId } = useSurveySession();
	return useRegisteredSurveyQuery({
		queryKey: surveyKeys.list(userId),
		enabled,
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
	const session = useSurveySession();
	return useRegisteredSurveyMutation({
		mutationFn: async (body: CreateSurveyCommand) => {
			const { data, error, response } = await client.POST("/api/survey", {
				body,
				signal: session.signal,
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
		onCacheSync: async (survey) => {
			const key = surveyKeys.detail(session.userId, survey.id);
			await cache.cancelQueries({ queryKey: key });
			session.requireReady();
			cache.setQueryData(key, survey);
			await cache.invalidateQueries({
				queryKey: surveyKeys.list(session.userId),
			});
		},
	});
}

export function useAnalyzeSurvey() {
	const client = useApiClient();
	const session = useSurveySession();
	return useRegisteredSurveyMutation({
		mutationFn: async (body: AnalyzeSurveyCommand) => {
			const { data, error, response } = await client.POST(
				"/api/survey/analyze",
				{ body, signal: session.signal },
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
	const session = useSurveySession();
	return useRegisteredSurveyMutation({
		mutationFn: async (id: number) => {
			const { response } = await client.DELETE("/api/survey/{id}", {
				params: { path: { id } },
				signal: session.signal,
			});
			if (!response.ok)
				throw new ApiError("Failed to delete survey", response.status);
			return id;
		},
		onCacheSync: async (id) => {
			await Promise.all([
				cache.cancelQueries({
					queryKey: surveyKeys.detail(session.userId, id),
				}),
				cache.cancelQueries({ queryKey: surveyKeys.list(session.userId) }),
			]);
			session.requireReady();
			// An active detail view must clear immediately, without refetching the deleted record.
			cache.setQueryData(surveyKeys.detail(session.userId, id), null);
			cache.setQueryData<UserSurveyModel[]>(
				surveyKeys.list(session.userId),
				(surveys) => surveys?.filter((survey) => survey.id !== id),
			);
			await cache.invalidateQueries({
				queryKey: surveyKeys.list(session.userId),
			});
		},
	});
}
