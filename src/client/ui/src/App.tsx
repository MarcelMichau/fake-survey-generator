import React, { useState, useEffect } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import NavBar from "./components/NavBar";
import CreateSurvey from "./components/CreateSurvey";
import GetSurvey from "./components/GetSurvey";
import MySurveys from "./components/MySurveys";
import Splash from "./components/Splash";
import Alert from "./components/Alert";
import Footer from "./components/Footer";
import * as Types from "./types";

const App: React.FC = () => {
    const [errorMessage, setErrorMessage] = useState("");
    const {
        getAccessTokenSilently,
        user,
        isAuthenticated,
        isLoading,
    } = useAuth0();

    const registerUser = async () => {
        const token = await getAccessTokenSilently();

        const response = await fetch(`/api/user/register`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`,
            },
        });

        const data: Types.UserResponse = await response.json();

        if (data.isError) {
            setErrorMessage(data.responseException.exceptionMessage.detail);
            return;
        }
    };

    const isUserRegistered = async (): Promise<boolean> => {
        const token = await getAccessTokenSilently();

        const response = await fetch(
            `/api/user/isRegistered?userId=${user.sub}`,
            {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            }
        );

        const data: Types.IsUserRegisteredResponse = await response.json();

        if (data.isError) {
            setErrorMessage(data.responseException.exceptionMessage.detail);
            return false;
        }

        return data.result.isUserRegistered;
    };

    useEffect(() => {
        const register = async () => {
            if (isAuthenticated && user) {
                const isUserAlreadyRegistered = await isUserRegistered();

                if (!isUserAlreadyRegistered) {
                    await registerUser();
                }
            }
        };

        register();
    }, [isAuthenticated, user]);

    return (
        <div className="flex flex-col h-full">
            <div className="flex-1">
                <NavBar />
                {!isAuthenticated && !isLoading ? (
                    <Splash />
                ) : (
                    <div className="grid sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 xl:grid-cols-2 gap-4 mb-4">
                        <div>
                            <CreateSurvey loading={isLoading} />
                        </div>
                        <div>
                            <GetSurvey loading={isLoading} />
                            {errorMessage !== "" && (
                                <Alert
                                    type="error"
                                    title="Oh no! Something did not go as planned."
                                    message={errorMessage}
                                ></Alert>
                            )}
                        </div>
                        <div>
                            <MySurveys loading={isLoading} />
                        </div>
                    </div>
                )}
            </div>
            <div className="flex-shrink-0">
                <Footer />
            </div>
        </div>
    );
};

export default App;
