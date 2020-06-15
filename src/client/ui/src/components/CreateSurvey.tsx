import React, { useState } from "react";
import * as Types from "../types";

type CreateSurveyProps = {
    onCreateSurvey: (command: Types.CreateSurveyCommand) => Promise<void>;
};

const CreateSurvey: React.FC<CreateSurveyProps> = ({
    onCreateSurvey,
}): React.ReactElement => {
    const [respondentType, setRespondentType] = useState("");
    const [topic, setTopic] = useState("");
    const [numberOfRespondents, setNumberOfRespondents] = useState(0);
    const [options, setOptions] = useState([
        { id: 1, optionText: "" },
    ] as Types.SurveyOptionModel[]);

    const updateOption = (optionId: number, optionText: string) => {
        setOptions(
            options.map((option) => {
                if (option.id !== optionId) return option;

                return {
                    ...option,
                    optionText,
                };
            })
        );
    };

    const onSubmit = (e: React.FormEvent<HTMLFormElement>): void => {
        e.preventDefault();
        const surveyCommand: Types.CreateSurveyCommand = {
            surveyTopic: topic,
            numberOfRespondents,
            respondentType,
            surveyOptions: options.map(
                (option) =>
                    ({
                        optionText: option.optionText,
                    } as Types.SurveyOptionDto)
            ),
        };

        onCreateSurvey(surveyCommand);

        setRespondentType("");
        setTopic("");
        setNumberOfRespondents(0);
        setOptions([{ id: 1, optionText: "" }] as Types.SurveyOptionModel[]);
    };

    return (
        <div>
            <form onSubmit={onSubmit} className="pure-form">
                <label>
                    Target Audience (Respondent Type)
                    <div>
                        <input
                            style={{ margin: "1em auto", color: "black" }}
                            type="text"
                            value={respondentType}
                            onChange={(e) => setRespondentType(e.target.value)}
                        />
                    </div>
                </label>
                <label>
                    Question (Survey Topic)
                    <div>
                        <input
                            style={{ margin: "1em auto", color: "black" }}
                            type="text"
                            value={topic}
                            onChange={(e) => setTopic(e.target.value)}
                        />
                    </div>
                </label>
                <label>
                    Number of Respondents
                    <div>
                        <input
                            style={{ margin: "1em auto", color: "black" }}
                            type="text"
                            value={numberOfRespondents}
                            onChange={(e) =>
                                setNumberOfRespondents(
                                    Number.isNaN(Number(e.target.value))
                                        ? numberOfRespondents
                                        : Number(e.target.value)
                                )
                            }
                        />
                    </div>
                </label>

                <span>Options</span>

                {options.map((option) => (
                    <div key={option.id}>
                        <label>
                            #{option.id}
                            <div>
                                <input
                                    style={{
                                        margin: "1em auto",
                                        color: "black",
                                    }}
                                    type="text"
                                    value={option.optionText}
                                    onChange={(e) =>
                                        updateOption(option.id, e.target.value)
                                    }
                                />
                            </div>
                        </label>
                    </div>
                ))}

                <div>
                    <button
                        style={{
                            margin: "1em",
                            background: "rgb(28, 184, 65)",
                            color: "white",
                        }}
                        className="pure-button"
                        type="button"
                        onClick={() =>
                            setOptions([
                                ...options,
                                { id: options.length + 1, optionText: "" },
                            ] as Types.SurveyOptionModel[])
                        }
                    >
                        + Add Option
                    </button>
                </div>

                <button
                    className="pure-button pure-button-primary"
                    type="submit"
                >
                    Create Survey
                </button>
            </form>
        </div>
    );
};

export default CreateSurvey;
