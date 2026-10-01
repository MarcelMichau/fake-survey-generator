import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import App from "./App.tsx";
import "./index.css";
import { Auth0Provider } from "@auth0/auth0-react";
import { ApiQueryProvider } from "./api/ApiQueryProvider";
import config from "./auth_config.json";

const rootElement = document.getElementById("root");

if (!rootElement) {
	throw new Error("Root element not found");
}

createRoot(rootElement).render(
	<StrictMode>
		<Auth0Provider
			domain={config.domain}
			clientId={config.clientId}
			authorizationParams={{
				redirect_uri: window.location.origin,
				audience: config.audience,
			}}
			useRefreshTokens={true}
			cacheLocation="localstorage"
		>
			<ApiQueryProvider>
				<App />
			</ApiQueryProvider>
		</Auth0Provider>
	</StrictMode>,
);
