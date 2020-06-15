import React from "react";
import * as Types from "../types";

const GetSurvey: React.FC<Types.GetSurveyProps> = ({
    surveyId,
    onUpdateSurveyId,
    onFetch,
    surveyDetail,
}) => {
    return (
        <div>
            <form className="pure-form">
                {" "}
                <label>
                    Survey ID
                    <div>
                        <input
                            style={{ margin: "1em auto", color: "black" }}
                            type="text"
                            value={surveyId}
                            onChange={(e) =>
                                onUpdateSurveyId(
                                    Number.isNaN(Number(e.target.value))
                                        ? surveyId
                                        : Number(e.target.value)
                                )
                            }
                        />
                    </div>
                </label>
            </form>

            <button
                className="pure-button pure-button-primary"
                onClick={onFetch}
            >
                Fetch
            </button>

            {surveyDetail.id > 0 && (
                <div
                    style={{
                        border: "2px solid white",
                        margin: "1em",
                        padding: "1em",
                    }}
                >
                    <h3>
                        This Survey Asked{" "}
                        <em>
                            {surveyDetail.numberOfRespondents}{" "}
                            {surveyDetail.respondentType}
                        </em>{" "}
                        the Question:
                    </h3>
                    <h2>
                        <em>{surveyDetail.topic}</em>
                    </h2>
                    <h3>And the results were:</h3>

                    <ol style={{ display: "inline-block", textAlign: "left" }}>
                        {surveyDetail.options
                            .sort((x, y) => y.numberOfVotes - x.numberOfVotes)
                            .map((option, index) => (
                                <li key={option.id}>
                                    {index === 0 ? (
                                        <strong
                                            style={{
                                                color: "rgb(28, 184, 65)",
                                            }}
                                        >
                                            {option.optionText} -{" "}
                                            {option.numberOfVotes} votes
                                        </strong>
                                    ) : (
                                        <span>
                                            {option.optionText} -{" "}
                                            {option.numberOfVotes} votes
                                        </span>
                                    )}
                                </li>
                            ))}
                    </ol>
                </div>
            )}
        </div>
    );
};

export default GetSurvey;
