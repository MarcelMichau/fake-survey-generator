import { useAuth0 } from "@auth0/auth0-react";
import createClient from "openapi-fetch";
import { useMemo } from "react";
import type { paths } from "../api/generated";

export function useApiClient() {
	const { getAccessTokenSilently } = useAuth0();
	return useMemo(() => {
		const client = createClient<paths>({ baseUrl: window.location.origin });
		client.use({
			async onRequest({ request }) {
				request.signal.throwIfAborted();
				const token = await getAccessTokenSilently();
				request.signal.throwIfAborted();
				request.headers.set("Authorization", `Bearer ${token}`);
				return request;
			},
		});
		return client;
	}, [getAccessTokenSilently]);
}
