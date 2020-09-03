import React, { useState } from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import { useAuth0 } from "@auth0/auth0-react";
import * as Types from "../types";
import SkeletonButton from "./SkeletonButton";
import Alert from "./Alert";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPaperPlane } from "@fortawesome/free-solid-svg-icons";

export type MySurveysProps = {
    loading: boolean;
};

const MySurveys: React.FC<MySurveysProps> = ({ loading }) => {
    const { getAccessTokenSilently } = useAuth0();
    const [userSurveys, setUserSurveys] = useState(
        [] as Types.UserSurveyModel[]
    );
    const [errorMessage, setErrorMessage] = useState("");

    const resetMessage = (): void => {
        setErrorMessage("");
    };

    const fetchSurveys = async () => {
        resetMessage();

        const token = await getAccessTokenSilently();

        const response = await fetch(`/api/survey/user`, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        });

        const data: Types.UserSurveyResponse = await response.json();

        if (data.isError) {
            setErrorMessage(data.responseException.exceptionMessage.detail);
            setUserSurveys([] as Types.UserSurveyModel[]);
            return;
        }

        const surveys = data.result;

        setUserSurveys(surveys);
    };

    const submitForm = async (e: React.FormEvent) => {
        e.preventDefault();
        await fetchSurveys();
    };

    return (
        <SkeletonTheme color="#2d3748" highlightColor="#667eea">
            <div className="dark:bg-gray-800 rounded px-8 pt-6 pb-8 mb-4">
                <h2 className="dark:text-indigo-500 text-xl font-semibold tracking-tight mb-2">
                    {loading ? (
                        <Skeleton width={100} />
                    ) : (
                        <span>My Surveys</span>
                    )}
                </h2>
                <form onSubmit={submitForm}>
                    <SkeletonButton onClick={submitForm} loading={loading}>
                        Get My Surveys
                        <FontAwesomeIcon icon={faPaperPlane} className="ml-1" />
                    </SkeletonButton>
                </form>

                {userSurveys.length > 0 && (
                    <table className="table-auto bg-gray-900 text-gray-400 border border-gray-700 my-4">
                        <thead>
                            <tr>
                                <th className="px-4 py-2">Question</th>
                                <th className="px-4 py-2">Audience</th>
                                <th className="px-4 py-2"># Respondents</th>
                                <th className="px-4 py-2"># Options</th>
                                <th className="px-4 py-2">Winning Option</th>
                                <th className="px-4 py-2">Winning # Votes</th>
                            </tr>
                        </thead>
                        <tbody>
                            {userSurveys.map((survey, index) => (
                                <tr key={survey.id}>
                                    <td className="border border-gray-700 px-4 py-2">
                                        {survey.topic}
                                    </td>
                                    <td className="border border-gray-700 px-4 py-2">
                                        {survey.respondentType}
                                    </td>
                                    <td className="border border-gray-700 px-4 py-2">
                                        {new Intl.NumberFormat().format(
                                            survey.numberOfRespondents
                                        )}
                                    </td>
                                    <td className="border border-gray-700 px-4 py-2">
                                        {survey.numberOfOptions}
                                    </td>
                                    <td className="border border-gray-700 px-4 py-2">
                                        {survey.winningOption}
                                    </td>
                                    <td className="border border-gray-700 px-4 py-2">
                                        {new Intl.NumberFormat().format(
                                            survey.winningOptionNumberOfVotes
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
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

export default MySurveys;
