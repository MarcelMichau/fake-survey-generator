import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [react()],
	server: {
		host: true, // Required for Playwright integration tests to work in CI
		proxy: {
			"/api": {
				target: process.env.API_HTTPS || process.env.API_HTTP,
				changeOrigin: true
			},
		},
	},
});
