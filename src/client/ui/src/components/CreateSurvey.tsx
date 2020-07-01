import React, { useState } from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import { useAuth0 } from "../react-auth0-spa";
import * as Types from "../types";
import Field from "./Field";
import Button from "./Button";
import SkeletonButton from "./SkeletonButton";
import Alert from "./Alert";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
    faPlus,
    faMinus,
    faPaperPlane,
} from "@fortawesome/free-solid-svg-icons";

type CreateSurveyProps = {
    loading: boolean;
};

const CreateSurvey: React.FC<CreateSurveyProps> = ({
    loading,
}): React.ReactElement => {
    const { getTokenSilently } = useAuth0();
    const [respondentType, setRespondentType] = useState("");
    const [topic, setTopic] = useState("");
    const [numberOfRespondents, setNumberOfRespondents] = useState(0);
    const [options, setOptions] = useState([
        { id: 1, optionText: "" },
    ] as Types.SurveyOptionModel[]);

    const [errorMessage, setErrorMessage] = useState("");
    const [successMessage, setSuccessMessage] = useState("");
    const [validationErrors, setValidationErrors] = useState([] as string[]);

    const resetMessages = (): void => {
        setSuccessMessage("");
        setErrorMessage("");
        setValidationErrors([]);
    };

    const createSurvey = async (surveyCommand: Types.CreateSurveyCommand) => {
        resetMessages();

        const token = await getTokenSilently();

        const response = await fetch(`/api/survey`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${token}`,
            },
            body: JSON.stringify(surveyCommand),
        });

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

        setSuccessMessage(
            `Survey created with ID: ${data.result.id}. Get the survey to see the outcome.`
        );
        setErrorMessage("");
        setValidationErrors([]);
    };

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

        createSurvey(surveyCommand);

        setRespondentType("");
        setTopic("");
        setNumberOfRespondents(0);
        setOptions([{ id: 1, optionText: "" }] as Types.SurveyOptionModel[]);
    };

    return (
        <SkeletonTheme color="#2d3748" highlightColor="#667eea">
            <div className="dark:bg-gray-800 rounded px-8 pt-6 pb-8 mb-4">
                <h2 className="dark:text-indigo-500 text-xl font-semibold tracking-tight mb-2">
                    {loading ? <Skeleton /> : <span>Create Survey</span>}
                </h2>
                <form onSubmit={onSubmit}>
                    <Field
                        label="Target Audience (Respondent Type)"
                        value={respondentType}
                        onChange={(value) => setRespondentType(value)}
                        loading={loading}
                        placeholder="Pragmatic Developers"
                    />
                    <Field
                        label="Question (Survey Topic)"
                        value={topic}
                        onChange={(value) => setTopic(value)}
                        loading={loading}
                        placeholder="Do you prefer tabs or spaces?"
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
                        loading={loading}
                    />
                    <span className="block text-gray-500">
                        {loading ? <Skeleton /> : <span>Options</span>}
                    </span>
                    {options.map((option, index) => (
                        <div key={option.id}>
                            <Field
                                label={`#${option.id}`}
                                value={option.optionText}
                                onChange={(value) =>
                                    updateOption(option.id, value)
                                }
                                loading={loading}
                                placeholder={
                                    index === 0
                                        ? "Most definitely tabs"
                                        : "Some other option"
                                }
                            >
                                {index > 0 && (
                                    <Button
                                        actionType="destructive"
                                        onClick={() => {
                                            setOptions([
                                                ...options.filter(
                                                    (o) => o.id !== option.id
                                                ),
                                            ]);
                                        }}
                                        additionalClasses={["lg:ml-4"]}
                                    >
                                        {`Remove #${option.id}`}
                                        <FontAwesomeIcon
                                            icon={faMinus}
                                            className="ml-1"
                                        />
                                    </Button>
                                )}
                            </Field>
                        </div>
                    ))}
                    <div className="my-2">
                        <SkeletonButton
                            onClick={() =>
                                setOptions([
                                    ...options,
                                    { id: options.length + 1, optionText: "" },
                                ] as Types.SurveyOptionModel[])
                            }
                            loading={loading}
                            actionType="secondary"
                        >
                            Add Option{" "}
                            <FontAwesomeIcon icon={faPlus} className="ml-1" />
                        </SkeletonButton>
                    </div>
                    <div className="my-2">
                        <SkeletonButton type="submit" loading={loading}>
                            Create Survey{" "}
                            <FontAwesomeIcon
                                icon={faPaperPlane}
                                className="ml-1"
                            />
                        </SkeletonButton>
                    </div>
                </form>
                <div>
                    {successMessage !== "" && (
                        <Alert
                            title="Survey Created"
                            message={successMessage}
                        ></Alert>
                    )}
                    {errorMessage !== "" && (
                        <Alert
                            type="error"
                            title="Oh no! Something did not go as planned."
                            message={errorMessage}
                        ></Alert>
                    )}
                    {validationErrors.map((error, index) => (
                        <Alert
                            key={index}
                            type="error"
                            title="Validation Error"
                            message={error}
                        ></Alert>
                    ))}
                </div>
            </div>
        </SkeletonTheme>
    );
};

export default CreateSurvey;
