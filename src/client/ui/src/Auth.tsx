import React from "react";
import { useAuth0 } from "./react-auth0-spa";

const Auth = () => {
    const {
        isAuthenticated,
        loginWithRedirect,
        logout,
        loading,
        user,
    } = useAuth0();

    return (
        <div>
            {loading && (
                <button
                    style={{
                        margin: "1em",
                        background: "rgb(28, 184, 65)",
                        color: "white",
                    }}
                    disabled
                    className="pure-button"
                    type="button"
                    onClick={() => loginWithRedirect({})}
                >
                    Logging in...
                </button>
            )}

            {!isAuthenticated && !loading && (
                <button
                    style={{
                        margin: "1em",
                        background: "rgb(28, 184, 65)",
                        color: "white",
                    }}
                    className="pure-button"
                    type="button"
                    onClick={() => loginWithRedirect({})}
                >
                    Log in
                </button>
            )}

            {isAuthenticated && !loading && (
                <button
                    style={{
                        margin: "1em",
                        background: "rgb(28, 184, 65)",
                        color: "white",
                    }}
                    className="pure-button"
                    type="button"
                    onClick={() => logout()}
                >
                    Log out ({user.name})
                </button>
            )}
        </div>
    );
};

export default Auth;
