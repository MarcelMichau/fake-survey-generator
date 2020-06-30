import React, { useState } from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import { useAuth0 } from "../react-auth0-spa";
import * as Types from "../types";
import Field from "./Field";
import Button from "./Button";
import Alert from "./Alert";
import SurveyResult from "./SurveyResult";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPaperPlane } from "@fortawesome/free-solid-svg-icons";

export type GetSurveyProps = {
    loading: boolean;
};

const GetSurvey: React.FC<GetSurveyProps> = ({ loading }) => {
    const { getTokenSilently } = useAuth0();
    const [surveyId, setSurveyId] = useState(0);
    const [surveyDetail, setSurveyDetail] = useState({} as Types.SurveyModel);
    const [errorMessage, setErrorMessage] = useState("");

    const fetchSurvey = async (surveyId: number) => {
        const token = await getTokenSilently();

        const response = await fetch(`/api/survey/${surveyId}`, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        });

        const data: Types.SurveyResponse = await response.json();

        if (data.isError) {
            setErrorMessage(data.responseException.exceptionMessage.detail);
            return;
        }

        const survey = data.result;

        setSurveyDetail(survey);
    };

    const submitForm = async (e: React.FormEvent) => {
        e.preventDefault();
        await fetchSurvey(surveyId);
    };

    return (
        <SkeletonTheme color="#2d3748" highlightColor="#667eea">
            <div className="dark:bg-gray-800 rounded px-8 pt-6 pb-8 mb-4">
                <h2 className="dark:text-indigo-500 text-xl font-semibold tracking-tight mb-2">
                    {loading ? (
                        <Skeleton width={100} />
                    ) : (
                        <span>Get Survey</span>
                    )}
                </h2>
                <form onSubmit={submitForm}>
                    <div className="mb-4">
                        <Field
                            label="Survey ID"
                            value={surveyId}
                            onChange={(value) =>
                                setSurveyId(
                                    Number.isNaN(Number(value))
                                        ? surveyId
                                        : Number(value)
                                )
                            }
                            loading={loading}
                        />
                    </div>
                    <Button onClick={submitForm} loading={loading}>
                        Get Survey
                        <FontAwesomeIcon icon={faPaperPlane} className="ml-1" />
                    </Button>
                </form>

                {surveyDetail.id > 0 && (
                    <SurveyResult surveyDetail={surveyDetail} />
                )}
                {errorMessage !== "" && (
                    <Alert
                        type="error"
                        title="Oh no! Something did not go as planned."
                        message={errorMessage}
                    ></Alert>
                )}
            </div>
        </SkeletonTheme>
    );
};

export default GetSurvey;
