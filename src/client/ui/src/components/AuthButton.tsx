import React from "react";
import { useAuth0 } from "@auth0/auth0-react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faSignInAlt, faSignOutAlt } from "@fortawesome/free-solid-svg-icons";

const AuthButton = () => {
    const {
        isAuthenticated,
        loginWithRedirect,
        logout,
        isLoading,
        user,
    } = useAuth0();

    const commonClasses =
        "inline-block text-sm px-4 py-2 leading-none border rounded text-white border-white hover:border-transparent hover:text-indigo-600 hover:bg-white mt-4 lg:mt-0";

    return (
        <span>
            {isLoading && (
                <button
                    disabled
                    className={`${commonClasses} cursor-not-allowed`}
                    type="button"
                    onClick={() => loginWithRedirect({})}
                >
                    Logging in...
                </button>
            )}

            {!isAuthenticated && !isLoading && (
                <button
                    className={commonClasses}
                    type="button"
                    onClick={() => loginWithRedirect({})}
                >
                    Log in / Register{" "}
                    <FontAwesomeIcon icon={faSignInAlt} className="ml-2" />
                </button>
            )}

            {isAuthenticated && !isLoading && (
                <button
                    className={commonClasses}
                    type="button"
                    onClick={() => logout({ returnTo: window.location.origin })}
                >
                    Log out ({user?.name}){" "}
                    <FontAwesomeIcon icon={faSignOutAlt} className="ml-2" />
                </button>
            )}
        </span>
    );
};

export default AuthButton;
