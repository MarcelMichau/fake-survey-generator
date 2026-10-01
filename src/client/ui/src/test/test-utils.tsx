import { QueryClientProvider } from "@tanstack/react-query";
import type { ReactElement } from "react";
import { render as browserRender } from "vitest-browser-react";
import { createQueryClient } from "../api/ApiQueryProvider";

/**
 * Render a component in Vitest Browser Mode and return its locator-based screen.
 */
export function render(ui: ReactElement) {
	const client = createQueryClient();
	return browserRender(ui, {
		wrapper: ({ children }) => (
			<QueryClientProvider client={client}>{children}</QueryClientProvider>
		),
	});
}
