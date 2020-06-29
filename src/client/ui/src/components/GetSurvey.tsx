import React from "react";
import * as Types from "../types";
import Field from "./Field";
import Button from "./Button";

const GetSurvey: React.FC<Types.GetSurveyProps> = ({
    surveyId,
    onUpdateSurveyId,
    onFetch,
    surveyDetail,
}) => {
    return (
        <div className="dark:bg-gray-800 rounded px-8 pt-6 pb-8 mb-4">
            <h2 className="dark:text-teal-600 text-xl font-semibold tracking-tight mb-2">
                Get Survey
            </h2>
            <form>
                <div className="mb-4">
                    <Field
                        label="Survey ID"
                        value={surveyId}
                        onChange={(value) =>
                            onUpdateSurveyId(
                                Number.isNaN(Number(value))
                                    ? surveyId
                                    : Number(value)
                            )
                        }
                    />
                </div>
                <Button
                    text="Get Survey"
                    onClick={(e) => {
                        e.preventDefault();
                        onFetch();
                    }}
                />
            </form>

            {surveyDetail.id > 0 && (
                <div className="max-w-sm w-full lg:max-w-full lg:flex my-5">
                    <div className="dark:bg-gray-900 border dark:border-gray-600 bg-white rounded p-4 flex flex-col justify-between leading-normal shadow-md">
                        <div className="mb-8">
                            <p className="whitespace-pre text-sm text-gray-500 flex items-center">
                                This Survey Asked
                                <span> </span>
                                <span className="font-bold dark:text-teal-600">
                                    {surveyDetail.numberOfRespondents}
                                    <span> </span>
                                    {surveyDetail.respondentType}
                                </span>
                                <span> </span>
                                the Question:
                            </p>
                            <div className="text-gray-300 font-bold text-xl mb-2">
                                {surveyDetail.topic}
                            </div>
                            <p className="text-sm text-gray-500 flex items-center mb-2">
                                And the results were:
                            </p>
                            <div className="text-gray-500 text-base">
                                {surveyDetail.options
                                    .sort(
                                        (x, y) =>
                                            y.numberOfVotes - x.numberOfVotes
                                    )
                                    .map((option, index) => (
                                        <p key={option.id} className="my-1">
                                            <span className="mr-2">
                                                #{index + 1})
                                            </span>
                                            <span>
                                                <span>{option.optionText}</span>
                                                <span
                                                    className={`inline-block bg-${
                                                        index === 0
                                                            ? "green"
                                                            : "orange"
                                                    }-200 rounded-full px-3 py-1 text-sm font-semibold text-${
                                                        index === 0
                                                            ? "green"
                                                            : "orange"
                                                    }-700 mx-2`}
                                                >
                                                    {option.numberOfVotes} votes
                                                </span>
                                            </span>
                                        </p>
                                    ))}
                            </div>
                        </div>
                        <div className="flex items-center">
                            <div className="text-sm">
                                <p className="text-gray-500">
                                    {new Intl.DateTimeFormat("en-ZA", {
                                        weekday: "long",
                                        year: "numeric",
                                        month: "long",
                                        day: "numeric",
                                    }).format(new Date(surveyDetail.createdOn))}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default GetSurvey;
