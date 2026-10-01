import { useAuth0 } from "@auth0/auth0-react";
import { useQuery } from "@tanstack/react-query";
import { ApiError } from "../api/ApiError";
import { useApiClient } from "./useApiClient";

export function useUserRegistration() {
	const client = useApiClient();
	const { user, isAuthenticated } = useAuth0();
	return useQuery({
		queryKey: ["user-registration", user?.sub],
		enabled: isAuthenticated && !!user?.sub,
		staleTime: Infinity,
		queryFn: async () => {
			// Registration is idempotent. Share the in-flight request across subscribers,
			// including React StrictMode's effect replay.
			const { data, response } = await client.POST("/api/user/register");
			if (!response.ok || !data) {
				throw new ApiError(
					"Oops, something went wrong with registering a user.",
					response.status,
				);
			}
			return data;
		},
	});
}
