import createClient from "openapi-fetch";
import type { paths } from "../api/generated";

type MockResponse = {
	status?: number;
	ok?: boolean;
	json?: () => Promise<unknown>;
};

// Keep component test responses at the transport boundary; query hooks run normally.
export function mockApiClient(
	apiCall: (url: string, options?: RequestInit) => Promise<MockResponse>,
) {
	return createClient<paths>({
		baseUrl: window.location.origin,
		fetch: async (request: Request) => {
			const url = new URL(request.url).pathname.slice(1);
			const body = await request.text();
			const result =
				request.method === "GET"
					? await apiCall(url)
					: await apiCall(url, {
							method: request.method,
							...(body ? { body } : {}),
						});
			const status = result.status ?? (result.ok === false ? 500 : 200);
			return new Response(
				status === 204 ? null : JSON.stringify(await result.json?.()),
				{
					status,
					headers: { "Content-Type": "application/json" },
				},
			);
		},
	});
}
