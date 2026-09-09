import type { ReactElement } from "react";
import { render as browserRender } from "vitest-browser-react";

/**
 * Render a component in Vitest Browser Mode and return its locator-based screen.
 */
export function render(ui: ReactElement) {
	return browserRender(ui);
}
