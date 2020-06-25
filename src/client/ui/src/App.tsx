import React, { useState, useEffect } from "react";
import { useAuth0 } from "./react-auth0-spa";
import Auth from "./Auth";
import CreateSurvey from "./components/CreateSurvey";
import GetSurvey from "./components/GetSurvey";
import VersionInfo from "./components/VersionInfo";
import * as Types from "./types";

const App: React.FC = () => {
    const [surveyId, setSurveyId] = useState(0);
    const [surveyDetail, setSurveyDetail] = useState({} as Types.SurveyModel);
    const [errorMessage, setErrorMessage] = useState("");
    const [validationErrors, setValidationErrors] = useState([] as string[]);
    const { getTokenSilently, user, isAuthenticated } = useAuth0();

    const registerUser = async () => {
        const token = await getTokenSilently();

        const response = await fetch(`/api/user/register`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`,
            },
        });

        if (response.status === 401) {
            setErrorMessage(
                "Unauthorised: You need to login in order to register"
            );
            return;
        }

        const data: Types.UserResponse = await response.json();

        if (data.isError) {
            setErrorMessage(data.responseException.exceptionMessage.detail);
            return;
        }
    };

    const isUserRegistered = async (): Promise<boolean> => {
        const token = await getTokenSilently();

        const response = await fetch(
            `/api/user/isRegistered?userId=${user.sub}`,
            {
                headers: {
                    Authorization: `Bearer ${token}`,
                },
            }
        );

        if (response.status === 401) {
            setErrorMessage(
                "Unauthorised: You need to log in to check if user is registered"
            );
            return false;
        }

        const data: Types.IsUserRegisteredResponse = await response.json();

        if (data.isError) {
            setErrorMessage(data.responseException.exceptionMessage.detail);
            return false;
        }

        return data.result;
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

    const fetchSurvey = async (surveyId: number) => {
        const token = await getTokenSilently();

        const response = await fetch(`/api/survey/${surveyId}`, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        });

        if (response.status === 401) {
            setErrorMessage(
                "Unauthorised: You need to log in to fetch surveys"
            );
            return;
        }

        const data: Types.SurveyResponse = await response.json();

        if (data.isError) {
            setErrorMessage(data.responseException.exceptionMessage.detail);
            return;
        }

        const survey = data.result;

        setSurveyDetail(survey);
    };

    const createSurvey = async (surveyCommand: Types.CreateSurveyCommand) => {
        const token = await getTokenSilently();

        const response = await fetch(`/api/survey`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`,
            },
            body: JSON.stringify(surveyCommand),
        });

        if (response.status === 401) {
            setErrorMessage(
                "Unauthorised: You need to log in to create surveys"
            );
            return;
        }

        const data: Types.SurveyResponse = await response.json();

        if (data.isError) {
            setErrorMessage(data.responseException.exceptionMessage.title);

            if (data.responseException.exceptionMessage.errors) {
                setValidationErrors(
                    Object.values(
                        data.responseException.exceptionMessage.errors
                    ).flat()
                );
            }

            return;
        }

        const survey = data.result;

        setSurveyId(survey.id);
        setSurveyDetail(survey);
        setErrorMessage("");
        setValidationErrors([]);
    };

    const errorStyle = {
        color: "red",
    };

    return (
        <div>
            <div style={{ display: "flex", justifyContent: "flex-end" }}>
                <Auth></Auth>
            </div>
            <div style={{ textAlign: "center" }}>
                <h1>Fake Survey Generator</h1>
                <VersionInfo></VersionInfo>
                {errorMessage !== "" && (
                    <p style={errorStyle}>{errorMessage}</p>
                )}
                <h2>Get Survey</h2>
                <GetSurvey
                    surveyId={surveyId}
                    onUpdateSurveyId={(value: number) => setSurveyId(value)}
                    onFetch={() => fetchSurvey(surveyId)}
                    surveyDetail={surveyDetail}
                />
                <div style={{ margin: "2em" }}>---- ¯\_(ツ)_/¯ ----</div>
                <h2>Create Survey</h2>
                <CreateSurvey onCreateSurvey={createSurvey} />
                <div style={errorStyle}>
                    {validationErrors.map((error, index) => (
                        <p key={index}>{error}</p>
                    ))}
                </div>
            </div>
        </div>
    );
};

export default App;
