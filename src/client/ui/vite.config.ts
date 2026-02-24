import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [react()],
	server: {
		host: true,
		port: Number.parseInt(process.env.PORT ?? "5173"),
		proxy: {
			"/api": {
				target: process.env.API_HTTPS || process.env.API_HTTP,
			},
		},
	},
});
