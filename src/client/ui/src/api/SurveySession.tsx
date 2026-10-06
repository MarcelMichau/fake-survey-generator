import {
	type QueryFunctionContext,
	type QueryKey,
	type UseMutationOptions,
	type UseQueryOptions,
	type UseQueryResult,
	useMutation,
	useQuery,
	useQueryClient,
} from "@tanstack/react-query";
import { createContext, type ReactNode, useContext, useState } from "react";
import { useApiClient } from "../hooks/useApiClient";
import type { UserModel } from "../types";
import { ApiError } from "./ApiError";

export const registrationKey = (userId: string | undefined) =>
	["user-registration", userId] as const;

export function SurveySession({
	children,
	userId,
	signal,
}: {
	children: ReactNode;
	userId: string | undefined;
	signal: AbortSignal;
}) {
	const client = useApiClient();
	const cache = useQueryClient();
	const registration = useQuery({
		queryKey: registrationKey(userId),
		enabled: !!userId,
		staleTime: Infinity,
		retry: false,
		retryOnMount: false,
		refetchOnWindowFocus: false,
		refetchOnReconnect: false,
		queryFn: async () => {
			if (!userId)
				throw new ApiError("User registration requires authentication.", 0);
			// Session cancellation, rather than observer cancellation, preserves the
			// shared registration attempt during StrictMode's effect replay.
			signal.throwIfAborted();
			const { data, response } = await client.POST("/api/user/register", {
				signal,
			});
			signal.throwIfAborted();
			if (!response.ok || !data) {
				throw new ApiError(
					"Oops, something went wrong with registering a user.",
					response.status,
				);
			}
			return data;
		},
	});

	function requireReady() {
		signal.throwIfAborted();
		// Consult the session's current state, not a previous render's snapshot.
		if (
			!userId ||
			cache.getQueryState(registrationKey(userId))?.status !== "success"
		) {
			throw new ApiError(
				"Survey operations are unavailable until user registration succeeds.",
				0,
			);
		}
	}

	return (
		<SessionContext.Provider
			value={{
				registration,
				userId,
				signal,
				isReady: !!userId && !signal.aborted && registration.isSuccess,
				requireReady,
			}}
		>
			{children}
		</SessionContext.Provider>
	);
}

type Session = {
	registration: UseQueryResult<UserModel, Error>;
	userId: string | undefined;
	signal: AbortSignal;
	isReady: boolean;
	requireReady: () => void;
};

const SessionContext = createContext<Session | null>(null);

export function useSurveySession() {
	const session = useContext(SessionContext);
	if (!session) throw new Error("Survey queries require an ApiQueryProvider.");
	return session;
}

/** Retains requested read intent while enforcing readiness, including manual refresh. */
export function useRegisteredSurveyQuery<T>(
	options: Omit<UseQueryOptions<T, Error>, "queryFn" | "enabled"> & {
		enabled: boolean;
		queryFn: (context: QueryFunctionContext<QueryKey>) => Promise<T>;
	},
) {
	const session = useSurveySession();
	const [refreshRequested, setRefreshRequested] = useState(false);
	const { queryFn, enabled, ...queryOptions } = options;
	const query = useQuery({
		...queryOptions,
		enabled: (enabled || refreshRequested) && session.isReady,
		queryFn: async (context) => {
			session.requireReady();
			const result = await queryFn(context);
			session.requireReady();
			return result;
		},
	});

	const refetch: typeof query.refetch = (settings) => {
		if (!session.isReady) {
			setRefreshRequested(true);
			return Promise.resolve(query);
		}
		return query.refetch(settings);
	};

	return { ...query, refetch };
}

/** Rejects early mutations and discards outcomes from a removed session. */
export function useRegisteredSurveyMutation<T, V>(
	options: UseMutationOptions<T, Error, V> & {
		mutationFn: NonNullable<UseMutationOptions<T, Error, V>["mutationFn"]>;
		onCacheSync?: UseMutationOptions<T, Error, V>["onSuccess"];
	},
) {
	const session = useSurveySession();
	const { onCacheSync, ...mutationOptions } = options;
	return useMutation({
		...mutationOptions,
		mutationFn: async (variables, context) => {
			session.requireReady();
			const result = await options.mutationFn(variables, context);
			session.requireReady();
			return result;
		},
		onSuccess: async (...args) => {
			session.requireReady();
			try {
				await onCacheSync?.(...args);
			} catch (error) {
				// Session changes must still discard accepted results; only cache
				// failures in a ready session are separate from mutation failure.
				session.requireReady();
				console.error(
					"Survey mutation succeeded, but cache synchronization failed.",
					error,
				);
			}
			session.requireReady();
			await options.onSuccess?.(...args);
			session.requireReady();
		},
	});
}
