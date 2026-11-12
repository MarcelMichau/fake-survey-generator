import react from "@vitejs/plugin-react";
import { playwright } from "@vitest/browser-playwright";
import { defineConfig } from "vitest/config";

const browser = process.env.UI_TEST_BROWSER ?? "chromium";
if (browser !== "chromium" && browser !== "firefox" && browser !== "webkit") {
	throw new Error(`Unsupported UI_TEST_BROWSER: ${browser}`);
}

export default defineConfig({
	plugins: [react()],
	test: {
		setupFiles: ["./src/test/setup.ts"],
		css: true,
		browser: {
			enabled: true,
			provider: playwright(),
			headless: true,
			instances: [{ browser }],
		},
	},
});
