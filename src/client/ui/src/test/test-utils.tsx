import { useAuth0 } from "@auth0/auth0-react";
import { QueryClientProvider } from "@tanstack/react-query";
import { type ReactElement, useEffect } from "react";
import { render as browserRender } from "vitest-browser-react";
import { createQueryClient } from "../api/ApiQueryProvider";
import { registrationKey, SurveySession } from "../api/SurveySession";

/**
 * Render a component in Vitest Browser Mode and return its locator-based screen.
 */
export function render(ui: ReactElement) {
	const client = createQueryClient();
	const cancellation = new AbortController();
	return browserRender(ui, {
		wrapper: ({ children }) => {
			useEffect(
				() => () => {
					cancellation.abort();
					client.clear();
				},
				[],
			);
			const { isAuthenticated, user } = useAuth0();
			const userId = isAuthenticated ? user?.sub : undefined;
			// Rendering tests start from an already-registered session; lifecycle
			// tests use the real ApiQueryProvider to exercise registration attempts.
			if (userId && !client.getQueryState(registrationKey(userId))) {
				client.setQueryData(registrationKey(userId), { id: 1 });
			}
			return (
				<QueryClientProvider client={client}>
					<SurveySession userId={userId} signal={cancellation.signal}>
						{children}
					</SurveySession>
				</QueryClientProvider>
			);
		},
	});
}
