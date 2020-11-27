import React from "react";
import ReactDOM from "react-dom";
import "./assets/main.css";
import App from "./App";
import * as serviceWorker from "./serviceWorker";
import { Auth0Provider } from "@auth0/auth0-react";
import config from "./auth_config.json";

ReactDOM.render(
    <Auth0Provider
        domain={config.domain}
        clientId={config.clientId}
        redirectUri={window.location.origin}
        audience={config.audience}
        useRefreshTokens={true}
        cacheLocation="localstorage"
    >
        <App />
    </Auth0Provider>,
    document.getElementById("root")
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
