import React, { useState } from "react";
import * as Types from "../types";
import Field from "./Field";
import Button from "./Button";

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
        <div className="dark:bg-gray-800 rounded px-8 pt-6 pb-8 mb-4">
            <h2 className="dark:text-teal-600 text-xl font-semibold tracking-tight mb-2">
                Create Survey
            </h2>
            <form onSubmit={onSubmit}>
                <Field
                    label="Target Audience (Respondent Type)"
                    value={respondentType}
                    onChange={(value) => setRespondentType(value)}
                />
                <Field
                    label="Question (Survey Topic)"
                    value={topic}
                    onChange={(value) => setTopic(value)}
                />
                <Field
                    label="Number of Respondents"
                    value={numberOfRespondents}
                    onChange={(value) =>
                        setNumberOfRespondents(
                            Number.isNaN(Number(value))
                                ? numberOfRespondents
                                : Number(value)
                        )
                    }
                />
                <span className="block text-gray-500 text-sm">Options</span>
                {options.map((option) => (
                    <div key={option.id}>
                        <Field
                            label={`#${option.id}`}
                            value={option.optionText}
                            onChange={(value) => updateOption(option.id, value)}
                        />
                    </div>
                ))}
                <div className="my-2 float-right">
                    <Button
                        text="+ Add Option"
                        onClick={() =>
                            setOptions([
                                ...options,
                                { id: options.length + 1, optionText: "" },
                            ] as Types.SurveyOptionModel[])
                        }
                    />
                </div>
                <div className="my-2">
                    <Button type="submit" text="Create Survey" />
                </div>
            </form>
        </div>
    );
};

export default CreateSurvey;
