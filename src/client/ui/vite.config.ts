import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import mkcert from "vite-plugin-mkcert";

// https://vitejs.dev/config/
export default defineConfig({
	plugins: [react(), mkcert()],
	server: {
		host: true,
		port: Number.parseInt(process.env.PORT ?? "5173"),
		proxy: {
			"/api": {
				target:
					process.env.services__fakesurveygeneratorapi__https__0 ||
					process.env.services__fakesurveygeneratorapi__http__0,
				changeOrigin: true,
				secure: false,
			},
		},
	},
});
