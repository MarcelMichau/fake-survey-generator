import React from "react";
import { useAuth0 } from "../react-auth0-spa";

const AuthButton = () => {
    const {
        isAuthenticated,
        loginWithRedirect,
        logout,
        loading,
        user,
    } = useAuth0();

    return (
        <span>
            {loading && (
                <button
                    disabled
                    className="inline-block cursor-not-allowed text-sm px-4 py-2 leading-none border rounded text-white border-white hover:border-transparent hover:text-indigo-600 hover:bg-white mt-4 lg:mt-0"
                    type="button"
                    onClick={() => loginWithRedirect({})}
                >
                    Logging in...
                </button>
            )}

            {!isAuthenticated && !loading && (
                <button
                    className="inline-block text-sm px-4 py-2 leading-none border rounded text-white border-white hover:border-transparent hover:text-indigo-600 hover:bg-white mt-4 lg:mt-0"
                    type="button"
                    onClick={() => loginWithRedirect({})}
                >
                    Log in / Register
                </button>
            )}

            {isAuthenticated && !loading && (
                <button
                    className="inline-block text-sm px-4 py-2 leading-none border rounded text-white border-white hover:border-transparent hover:text-indigo-600 hover:bg-white mt-4 lg:mt-0"
                    type="button"
                    onClick={() => logout()}
                >
                    Log out ({user.name})
                </button>
            )}
        </span>
    );
};

export default AuthButton;
