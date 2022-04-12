import React from "react";
import { SurveyModel } from "../types";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCalendarAlt } from "@fortawesome/free-solid-svg-icons";

type SurveyResultProps = {
    surveyDetail: SurveyModel;
};

const SurveyResult = ({ surveyDetail }: SurveyResultProps) => {
    return (
        <div className="max-w-sm w-full lg:max-w-full lg:flex my-5">
            <div className="dark:bg-gray-900 border dark:border-gray-600 bg-white rounded p-4 flex flex-col justify-between leading-normal shadow-md">
                <div className="mb-6">
                    <p className="text-sm text-gray-500 flex items-center">
                        <span>
                            This survey asked{" "}
                            <span className="font-bold dark:text-gray-400">
                                {new Intl.NumberFormat().format(
                                    surveyDetail.numberOfRespondents
                                )}
                            </span>{" "}
                            <span className="font-bold dark:text-indigo-500">
                                {surveyDetail.respondentType}
                            </span>{" "}
                            the question:
                        </span>
                    </p>
                    <div className="text-gray-300 font-bold text-xl mb-2">
                        {surveyDetail.topic}
                    </div>
                    <p className="text-sm text-gray-500 flex items-center mb-2">
                        And the results were:
                    </p>
                    <div className="text-gray-500 text-base">
                        {surveyDetail.options
                            .sort((x, y) => y.numberOfVotes - x.numberOfVotes)
                            .map((option, index) => (
                                <p key={option.id} className="my-1">
                                    <span className="mr-2">#{index + 1})</span>
                                    <span>
                                        <span>{option.optionText}</span>
                                        <span
                                            className={`inline-block ${
                                                index === 0
                                                    ? "bg-green-200"
                                                    : "bg-yellow-200"
                                            } rounded-full px-3 py-1 text-sm font-semibold ${
                                                index === 0
                                                    ? "text-green-700"
                                                    : "text-yellow-700"
                                            } mx-2`}
                                        >
                                            {new Intl.NumberFormat().format(
                                                option.numberOfVotes
                                            )}{" "}
                                            votes
                                        </span>
                                    </span>
                                </p>
                            ))}
                    </div>
                </div>
                <div className="text-sm">
                    <p className="text-gray-500">
                        <FontAwesomeIcon
                            icon={faCalendarAlt}
                            className="mr-2"
                        />
                        {new Intl.DateTimeFormat("default", {
                            weekday: "long",
                            year: "numeric",
                            month: "long",
                            day: "numeric",
                        }).format(new Date(surveyDetail.createdOn))}
                    </p>
                </div>
            </div>
        </div>
    );
};

export default SurveyResult;
