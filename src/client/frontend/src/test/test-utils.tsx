import { render as rtlRender } from "@testing-library/react";
import type { ReactElement } from "react";

/**
 * Custom render function that wraps components with necessary providers
 */
export function render(ui: ReactElement) {
  return rtlRender(ui);
}

export * from "@testing-library/react";
export { default as userEvent } from "@testing-library/user-event";
