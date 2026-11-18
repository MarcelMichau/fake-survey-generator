import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [react()],
	server: {
		// Allow Tailscale hostnames for remote access (docs/running-on-a-tailnet.md).
		allowedHosts: [".ts.net"],
		proxy: {
			"/api": {
				target: process.env.API_HTTPS || process.env.API_HTTP,
				changeOrigin: true,
			},
		},
	},
});
