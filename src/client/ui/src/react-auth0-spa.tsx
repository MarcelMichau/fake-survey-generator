import React, { useState, useEffect, useContext } from "react";
import createAuth0Client, { Auth0Client, IdToken } from "@auth0/auth0-spa-js";

interface Auth0ProviderOptions {
    children: any;
    onRedirectCallback?: (appState: any) => void;
    domain: string;
    client_id: string;
    redirect_uri: string;
    audience: string;
}

interface UseAuth0Context {
    isAuthenticated: boolean;
    user?: any;
    loading: boolean;
    popupOpen: boolean;
    loginWithPopup: (params?: {}) => Promise<void>;
    handleRedirectCallback: () => Promise<void>;
    getIdTokenClaims: (...p: any) => Promise<IdToken> | undefined;
    loginWithRedirect: (...p: any) => Promise<void> | undefined;
    getTokenSilently: (...p: any) => Promise<any> | undefined;
    getTokenWithPopup: (...p: any) => Promise<string> | undefined;
    logout: (...p: any) => void | undefined;
}

const DEFAULT_REDIRECT_CALLBACK = () =>
    window.history.replaceState({}, document.title, window.location.pathname);

export const Auth0Context = React.createContext<UseAuth0Context>(
    {} as UseAuth0Context
);
export const useAuth0 = () => useContext(Auth0Context);
export const Auth0Provider = ({
    children,
    onRedirectCallback = DEFAULT_REDIRECT_CALLBACK,
    ...initOptions
}: Auth0ProviderOptions) => {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [user, setUser] = useState();
    const [auth0Client, setAuth0] = useState<Auth0Client>();
    const [loading, setLoading] = useState(true);
    const [popupOpen, setPopupOpen] = useState(false);

    useEffect(() => {
        const initAuth0 = async () => {
            const auth0FromHook = await createAuth0Client(initOptions);
            setAuth0(auth0FromHook);

            if (
                window.location.search.includes("code=") &&
                window.location.search.includes("state=")
            ) {
                const {
                    appState,
                } = await auth0FromHook.handleRedirectCallback();
                onRedirectCallback(appState);
            }

            const isAuthenticated = await auth0FromHook.isAuthenticated();

            setIsAuthenticated(isAuthenticated);

            if (isAuthenticated) {
                const user = await auth0FromHook.getUser();
                setUser(user);
            }

            setLoading(false);
        };
        initAuth0();
        // eslint-disable-next-line
    }, []);

    const loginWithPopup = async (params = {}) => {
        setPopupOpen(true);
        try {
            await auth0Client?.loginWithPopup(params);
        } catch (error) {
            console.error(error);
        } finally {
            setPopupOpen(false);
        }
        const user = await auth0Client?.getUser();
        setUser(user);
        setIsAuthenticated(true);
    };

    const handleRedirectCallback = async () => {
        setLoading(true);
        await auth0Client?.handleRedirectCallback();
        const user = await auth0Client?.getUser();
        setLoading(false);
        setIsAuthenticated(true);
        setUser(user);
    };
    return (
        <Auth0Context.Provider
            value={{
                isAuthenticated,
                user,
                loading,
                popupOpen,
                loginWithPopup,
                handleRedirectCallback,
                getIdTokenClaims: (...p: any) =>
                    auth0Client?.getIdTokenClaims(...p),
                loginWithRedirect: (...p: any) =>
                    auth0Client?.loginWithRedirect(...p),
                getTokenSilently: (...p: any) =>
                    auth0Client?.getTokenSilently(...p),
                getTokenWithPopup: (...p: any) =>
                    auth0Client?.getTokenWithPopup(...p),
                logout: (...p: any) => auth0Client?.logout(...p),
            }}
        >
            {children}
        </Auth0Context.Provider>
    );
};
