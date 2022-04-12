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

const MySurveys = ({ loading }: MySurveysProps) => {
    const { getAccessTokenSilently } = useAuth0();
    const [userSurveysResponse, setUserSurveysResponse] = useState(
        {} as Types.UserSurveysResponse
    );

    const fetchSurveys = async () => {
        const token = await getAccessTokenSilently();

        const response = await fetch(`/api/survey/user`, {
            headers: {
                Authorization: `Bearer ${token}`,
            },
        });

        const data: Types.UserSurveysResponse = await response.json();

        setUserSurveysResponse(data);
    };

    const submitForm = async (e: React.FormEvent) => {
        e.preventDefault();
        await fetchSurveys();
    };

    const tablePadding = "px-4 py-2";
    const tableBorder = "border border-gray-700";

    type TableHeaderProps = { children: React.ReactNode; };
    const TableHeader = ({ children } : TableHeaderProps) => (
        <th className={`${tablePadding}`}>{children}</th>
    );

    type TableDataProps = { children: React.ReactNode; }
    const TableData = ({ children }: TableDataProps) => (
        <td className={`${tableBorder} ${tablePadding}`}>{children}</td>
    );

    return (
        <SkeletonTheme baseColor="#2d3748" highlightColor="#667eea">
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

                {!userSurveysResponse?.isError &&
                    userSurveysResponse?.result?.length > 0 && (
                        <table
                            className={`table-auto bg-gray-900 text-gray-400 ${tableBorder} my-4`}
                        >
                            <thead>
                                <tr>
                                    <TableHeader>Question</TableHeader>
                                    <TableHeader>Audience</TableHeader>
                                    <TableHeader># Respondents</TableHeader>
                                    <TableHeader># Options</TableHeader>
                                    <TableHeader>Winning Option</TableHeader>
                                    <TableHeader>Winning # Votes</TableHeader>
                                </tr>
                            </thead>
                            <tbody>
                                {userSurveysResponse.result.map((survey) => (
                                    <tr key={survey.id}>
                                        <TableData>{survey.topic}</TableData>
                                        <TableData>
                                            {survey.respondentType}
                                        </TableData>
                                        <TableData>
                                            {new Intl.NumberFormat().format(
                                                survey.numberOfRespondents
                                            )}
                                        </TableData>
                                        <TableData>
                                            {survey.numberOfOptions}
                                        </TableData>
                                        <TableData>
                                            {survey.winningOption}
                                        </TableData>
                                        <TableData>
                                            {new Intl.NumberFormat().format(
                                                survey.winningOptionNumberOfVotes
                                            )}
                                        </TableData>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    )}
                {!userSurveysResponse?.isError &&
                    userSurveysResponse?.result?.length === 0 && (
                        <Alert
                            title="No Surveys"
                            message={"You have not created any surveys yet. :("}
                        ></Alert>
                    )}
                {userSurveysResponse?.isError && (
                    <Alert
                        type="error"
                        title="Oh no! Something did not go as planned."
                        message={
                            userSurveysResponse.responseException
                                .exceptionMessage.detail
                        }
                    ></Alert>
                )}
            </div>
        </SkeletonTheme>
    );
};

export default MySurveys;
