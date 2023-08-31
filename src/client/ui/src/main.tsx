import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import "./index.css";
import config from "./auth_config.json";
import { Auth0Provider } from "@auth0/auth0-react";

ReactDOM.createRoot(document.getElementById("root")!).render(
    <React.StrictMode>
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
            <App />
        </Auth0Provider>
    </React.StrictMode>
);
