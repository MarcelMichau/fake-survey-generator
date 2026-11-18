import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [react()],
	server: {
		// Listen on IPv4 loopback only: the Tailscale Service forwards to 127.0.0.1, and
		// "localhost" may resolve to ::1. Allow Tailscale hostnames (docs/running-on-a-tailnet.md).
		host: "127.0.0.1",
		allowedHosts: [".ts.net"],
		proxy: {
			"/api": {
				target: process.env.API_HTTPS || process.env.API_HTTP,
				changeOrigin: true,
			},
		},
	},
});
