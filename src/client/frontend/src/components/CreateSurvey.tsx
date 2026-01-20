import type React from "react";
import { useState, useCallback, useRef } from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import type * as Types from "../types";
import Field from "./Field";
import Button from "./Button";
import SkeletonButton from "./SkeletonButton";
import Alert from "./Alert";
import { useApiCall } from "../hooks";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
	faPlus,
	faMinus,
	faPaperPlane,
} from "@fortawesome/free-solid-svg-icons";

type CreateSurveyProps = {
	loading: boolean;
	onSurveyCreated?: (surveyId: number) => void;
};

// Form option type - doesn't include numberOfVotes which is only on response
interface FormSurveyOption {
	id: number;
	optionText: string;
	preferredNumberOfVotes: number;
}

// Consolidated form state interface
interface SurveyFormState {
	survey: {
		respondentType: string;
		topic: string;
		numberOfRespondents: number;
		options: FormSurveyOption[];
	};
	messages: {
		success: string;
		error: string;
		validationErrors: string[];
	};
	ui: {
		isSubmitting: boolean;
	};
}

const initialFormState: SurveyFormState = {
	survey: {
		respondentType: "",
		topic: "",
		numberOfRespondents: 0,
		options: [{ id: 1, optionText: "", preferredNumberOfVotes: 0 }],
	},
	messages: {
		success: "",
		error: "",
		validationErrors: [],
	},
	ui: {
		isSubmitting: false,
	},
};

const CreateSurvey = ({
	loading,
	onSurveyCreated,
}: CreateSurveyProps): React.ReactElement => {
	const { apiCall } = useApiCall();
	const [formState, setFormState] = useState<SurveyFormState>(initialFormState);
	// Counter for generating unique option IDs - never decreases
	const nextOptionIdRef = useRef(2);

	const updateSurveyField = useCallback(
		<K extends keyof SurveyFormState["survey"]>(
			key: K,
			value: SurveyFormState["survey"][K],
		) => {
			setFormState((prev) => ({
				...prev,
				survey: {
					...prev.survey,
					[key]: value,
				},
			}));
		},
		[],
	);

	const resetMessages = useCallback(() => {
		setFormState((prev) => ({
			...prev,
			messages: {
				success: "",
				error: "",
				validationErrors: [],
			},
		}));
	}, []);

	const resetForm = useCallback(() => {
		setFormState(initialFormState);
		nextOptionIdRef.current = 2; // Reset counter to 2 (next ID after initial option with ID 1)
	}, []);

	const updateOption = useCallback((optionId: number, optionText: string) => {
		setFormState((prev) => ({
			...prev,
			survey: {
				...prev.survey,
				options: prev.survey.options.map((option) =>
					option.id === optionId ? { ...option, optionText } : option,
				),
			},
		}));
	}, []);

	const updatePreferredVotes = useCallback(
		(optionId: number, preferredVotes: number) => {
			setFormState((prev) => ({
				...prev,
				survey: {
					...prev.survey,
					options: prev.survey.options.map((option) =>
						option.id === optionId
							? { ...option, preferredNumberOfVotes: preferredVotes }
							: option,
					),
				},
			}));
		},
		[],
	);

	const removeOption = useCallback((optionId: number) => {
		setFormState((prev) => ({
			...prev,
			survey: {
				...prev.survey,
				options: prev.survey.options.filter((o) => o.id !== optionId),
			},
		}));
	}, []);

	const addOption = useCallback(() => {
		setFormState((prev) => {
			const newId = nextOptionIdRef.current;
			nextOptionIdRef.current += 1;
			return {
				...prev,
				survey: {
					...prev.survey,
					options: [
						...prev.survey.options,
						{
							id: newId,
							optionText: "",
							preferredNumberOfVotes: 0,
						},
					],
				},
			};
		});
	}, []);

	const createSurvey = useCallback(
		async (surveyCommand: Types.CreateSurveyCommand) => {
			resetMessages();
			setFormState((prev) => ({
				...prev,
				ui: { isSubmitting: true },
			}));

			try {
				const response = await apiCall("api/survey", {
					method: "POST",
					body: JSON.stringify(surveyCommand),
				});

				if (response.status === 422) {
					const data: Record<string, string[]> = await response.json();
					setFormState((prev) => ({
						...prev,
						messages: {
							...prev.messages,
							validationErrors: Object.values(data).flat(),
						},
						ui: { isSubmitting: false },
					}));
					return;
				}

				if (response.status !== 201) {
					setFormState((prev) => ({
						...prev,
						messages: {
							...prev.messages,
							error: "Please try again or create an issue on GitHub",
						},
						ui: { isSubmitting: false },
					}));
					return;
				}

				const data: Types.SurveyModel = await response.json();

				setFormState((prev) => ({
					...prev,
					messages: {
						success: `Survey created with ID: ${data.id}. Get the survey to see the outcome.`,
						error: "",
						validationErrors: [],
					},
					ui: { isSubmitting: false },
				}));

				resetForm();

				if (onSurveyCreated) {
					onSurveyCreated(data.id);
				}
			} catch (error) {
				setFormState((prev) => ({
					...prev,
					messages: {
						...prev.messages,
						error: "An unexpected error occurred",
					},
					ui: { isSubmitting: false },
				}));
			}
		},
		[apiCall, resetMessages, resetForm, onSurveyCreated],
	);

	const onSubmit = async (
		e: React.FormEvent<HTMLFormElement>,
	): Promise<void> => {
		e.preventDefault();
		const surveyCommand: Types.CreateSurveyCommand = {
			surveyTopic: formState.survey.topic,
			numberOfRespondents: formState.survey.numberOfRespondents,
			respondentType: formState.survey.respondentType,
			surveyOptions: formState.survey.options.map(
				(option) =>
					({
						optionText: option.optionText,
						preferredNumberOfVotes: option.preferredNumberOfVotes,
					}) as Types.SurveyOptionDto,
			),
		};

		await createSurvey(surveyCommand);
	};

	return (
		<SkeletonTheme baseColor="#2d3748" highlightColor="#667eea">
			<div className="dark:bg-gray-800 rounded-sm px-8 pt-6 pb-8 mb-4">
				<h2 className="dark:text-indigo-500 text-xl font-semibold tracking-tight mb-2">
					{loading ? <Skeleton /> : <span>Create Survey</span>}
				</h2>
				<form onSubmit={onSubmit}>
					<Field
						label="Target Audience (Respondent Type)"
						value={formState.survey.respondentType}
						onChange={(value) => updateSurveyField("respondentType", value)}
						loading={loading}
						placeholder="Pragmatic Developers"
					/>
					<Field
						label="Question (Survey Topic)"
						value={formState.survey.topic}
						onChange={(value) => updateSurveyField("topic", value)}
						loading={loading}
						placeholder="Do you prefer tabs or spaces?"
					/>
					<Field
						label="Number of Respondents"
						value={formState.survey.numberOfRespondents}
						onChange={(value) =>
							updateSurveyField(
								"numberOfRespondents",
								Number.isNaN(Number(value))
									? formState.survey.numberOfRespondents
									: Number(value),
							)
						}
						loading={loading}
					/>
					<span className="block text-gray-500">
						{loading ? <Skeleton /> : <span>Options</span>}
					</span>
					{formState.survey.options.map((option, index) => (
						<div key={option.id}>
							<Field
								label={`#${option.id}`}
								value={option.optionText}
								onChange={(value) => updateOption(option.id, value)}
								loading={loading}
								placeholder={
									index === 0 ? "Most definitely tabs" : "Some other option"
								}
							>
								{index > 0 && (
									<Button
										actionType="destructive"
										onClick={() => removeOption(option.id)}
										additionalClasses={["lg:ml-4"]}
									>
										{`Remove #${option.id}`}
										<FontAwesomeIcon icon={faMinus} className="ml-1" />
									</Button>
								)}
							</Field>
							<div className="ml-4 mt-1 mb-3">
								<label
									htmlFor={`preferred-votes-${option.id}`}
									className="block text-gray-500 text-sm mb-1"
								>
									{loading ? <Skeleton width={100} /> : "Preferred Votes"}
								</label>
								<input
									id={`preferred-votes-${option.id}`}
									type="number"
									min="0"
									max={formState.survey.numberOfRespondents}
									value={option.preferredNumberOfVotes}
									onChange={(e) => {
										const value = Number.parseInt(e.target.value, 10);
										updatePreferredVotes(
											option.id,
											Number.isNaN(value) ? 0 : value,
										);
									}}
									disabled={loading}
									className="bg-gray-700 focus:outline-none focus:shadow-outline border border-gray-700 rounded py-1 px-2 block w-32 appearance-none leading-normal text-gray-200 focus:border-indigo-500"
								/>
								<p className="text-gray-500 text-xs mt-1">
									{loading ? (
										<Skeleton width={200} />
									) : (
										`Set to 0 for random distribution or specify the desired number of votes (max: ${formState.survey.numberOfRespondents})`
									)}
								</p>
							</div>
						</div>
					))}
					<div className="my-2">
						<SkeletonButton
							onClick={addOption}
							loading={loading}
							actionType="secondary"
						>
							Add Option <FontAwesomeIcon icon={faPlus} className="ml-1" />
						</SkeletonButton>
					</div>
					<div className="my-2">
						<SkeletonButton type="submit" loading={loading}>
							Create Survey{" "}
							<FontAwesomeIcon icon={faPaperPlane} className="ml-1" />
						</SkeletonButton>
					</div>
				</form>
				<div>
					{formState.messages.success !== "" && (
						<Alert title="Survey Created" message={formState.messages.success} />
					)}
					{formState.messages.error !== "" && (
						<Alert
							type="error"
							title="Oh no! Something did not go as planned."
							message={formState.messages.error}
						/>
					)}
					{formState.messages.validationErrors.map((error, index) => (
						<Alert
							// biome-ignore lint/suspicious/noArrayIndexKey: no unique identifier available
							key={index}
							type="error"
							title="Validation Error"
							message={error}
						/>
					))}
				</div>
			</div>
		</SkeletonTheme>
	);
};

export default CreateSurvey;
