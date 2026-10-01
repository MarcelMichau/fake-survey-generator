import {
	faMicroscope,
	faMinus,
	faPaperPlane,
	faPlus,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import type React from "react";
import { useCallback, useRef, useState } from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import { ApiError } from "../api/ApiError";
import { useAnalyzeSurvey, useCreateSurvey } from "../hooks/useSurveys";
import type * as Types from "../types";
import Alert from "./Alert";
import Button from "./Button";
import Field from "./Field";
import SkeletonButton from "./SkeletonButton";

type CreateSurveyProps = {
	loading: boolean;
	onSurveyCreated?: (surveyId: number) => void;
	resetOnSuccess?: boolean;
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
		isCreated: boolean;
	};
	analysis: {
		completed: boolean;
		warnings: Types.SurveyAnalysisWarningModel[];
		hasAcknowledgedWarnings: boolean;
		error: string;
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
		isCreated: false,
	},
	analysis: {
		completed: false,
		warnings: [],
		hasAcknowledgedWarnings: false,
		error: "",
	},
};

const CreateSurvey = ({
	loading,
	onSurveyCreated,
	resetOnSuccess = !onSurveyCreated,
}: CreateSurveyProps): React.ReactElement => {
	const creation = useCreateSurvey();
	const analysis = useAnalyzeSurvey();
	const [formState, setFormState] = useState<SurveyFormState>(initialFormState);
	// Counter for generating unique option IDs used as React keys. Display labels
	// are based on each option's current position instead.
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
				analysis: initialFormState.analysis,
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
			analysis: initialFormState.analysis,
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
				analysis: initialFormState.analysis,
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
			analysis: initialFormState.analysis,
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
				analysis: initialFormState.analysis,
			};
		});
	}, []);

	const createSurvey = async (surveyCommand: Types.CreateSurveyCommand) => {
		resetMessages();
		try {
			const data = await creation.mutateAsync(surveyCommand);
			if (resetOnSuccess) resetForm();
			setFormState((prev) => ({
				...prev,
				messages: {
					success: `Survey created with ID: ${data.id}. Get the survey to see the outcome.`,
					error: "",
					validationErrors: [],
				},
				ui: { isCreated: !resetOnSuccess },
				analysis: initialFormState.analysis,
			}));
			onSurveyCreated?.(data.id);
		} catch (error) {
			setFormState((prev) => ({
				...prev,
				messages: {
					...prev.messages,
					error:
						error instanceof ApiError
							? error.validationErrors.length > 0
								? ""
								: error.message
							: "An unexpected error occurred",
					validationErrors:
						error instanceof ApiError ? error.validationErrors : [],
				},
			}));
		}
	};

	const buildSurveyCommand = useCallback(
		(): Types.CreateSurveyCommand => ({
			surveyTopic: formState.survey.topic,
			numberOfRespondents: formState.survey.numberOfRespondents,
			respondentType: formState.survey.respondentType,
			surveyOptions: formState.survey.options.map(
				(option) =>
					({
						optionText: option.optionText,
						preferredNumberOfVotes: option.preferredNumberOfVotes,
					}) satisfies Types.SurveyOptionDto,
			),
		}),
		[formState.survey],
	);

	const analyzeSurvey = async () => {
		resetMessages();
		setFormState((prev) => ({ ...prev, analysis: initialFormState.analysis }));
		const { numberOfRespondents: _respondents, ...command } =
			buildSurveyCommand();
		try {
			const data = await analysis.mutateAsync(command);
			setFormState((prev) => ({
				...prev,
				analysis: {
					...initialFormState.analysis,
					completed: true,
					warnings: data.warnings,
				},
			}));
		} catch (error) {
			setFormState((prev) => ({
				...prev,
				analysis: {
					...initialFormState.analysis,
					error:
						error instanceof ApiError
							? error.validationErrors.join(" ") || error.message
							: "An unexpected error occurred while analyzing the survey.",
				},
			}));
		}
	};

	const onSubmit = async (
		e: React.SubmitEvent<HTMLFormElement>,
	): Promise<void> => {
		e.preventDefault();
		await createSurvey(buildSurveyCommand());
	};

	return (
		<SkeletonTheme baseColor="#30353a" highlightColor="#c7ff18">
			<div className="brutal-panel h-full">
				<h2 className="display-title text-4xl lg:text-5xl mb-5">
					{loading ? <Skeleton /> : <span>Create Survey</span>}
				</h2>
				<form
					onSubmit={onSubmit}
					aria-busy={analysis.isPending || creation.isPending}
				>
					<fieldset
						disabled={loading || analysis.isPending || creation.isPending}
					>
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
						<span className="ui-label">
							{loading ? <Skeleton /> : <span>Options</span>}
						</span>
						{formState.survey.options.map((option, index) => {
							const optionNumber = index + 1;

							return (
								<div key={option.id}>
									<Field
										label={`#${optionNumber}`}
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
												additionalClasses={["text-base!"]}
											>
												{`Remove #${optionNumber}`}
												<FontAwesomeIcon icon={faMinus} className="ml-1" />
											</Button>
										)}
									</Field>
									<div className="mt-1 mb-5">
										<label
											htmlFor={`preferred-votes-${option.id}`}
											className="ui-label"
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
											className="brutal-input max-w-40"
										/>
										<p className="text-gray-300 text-xs mt-2">
											{loading ? (
												<Skeleton width={200} />
											) : (
												`Set to 0 for random distribution or specify the desired number of votes (max: ${formState.survey.numberOfRespondents})`
											)}
										</p>
									</div>
								</div>
							);
						})}
						<div className="my-2">
							<SkeletonButton
								onClick={addOption}
								loading={loading}
								actionType="secondary"
							>
								Add Option <FontAwesomeIcon icon={faPlus} className="ml-1" />
							</SkeletonButton>
						</div>
						<div className="mt-6 border-t-2 border-white pt-4">
							{formState.analysis.warnings.length > 0 && (
								<label className="my-3 flex items-start gap-3 text-sm text-paper">
									<input
										type="checkbox"
										checked={formState.analysis.hasAcknowledgedWarnings}
										onChange={(event) =>
											setFormState((prev) => ({
												...prev,
												analysis: {
													...prev.analysis,
													hasAcknowledgedWarnings: event.target.checked,
												},
											}))
										}
										disabled={creation.isPending}
									/>
									I confirm that I want to create this survey with bad data
									&amp; that I feel bad about it
								</label>
							)}
							<div className="my-2">
								<SkeletonButton
									type="submit"
									loading={loading}
									disabled={
										creation.isPending ||
										analysis.isPending ||
										(formState.analysis.warnings.length > 0 &&
											!formState.analysis.hasAcknowledgedWarnings)
									}
								>
									{formState.analysis.warnings.length > 0
										? "Create Survey (Despite All The Issues Identified)"
										: "Create Survey"}{" "}
									<FontAwesomeIcon icon={faPaperPlane} className="ml-1" />
								</SkeletonButton>
							</div>
						</div>
						<div className="mt-4">
							<p className="mb-2 text-sm text-gray-300">
								Analyse provided survey information for potential issues.
							</p>
							<SkeletonButton
								onClick={() => void analyzeSurvey()}
								loading={loading}
								disabled={
									creation.isPending ||
									analysis.isPending ||
									formState.analysis.completed ||
									formState.ui.isCreated
								}
								actionType="secondary"
							>
								{analysis.isPending ? "Analysing..." : "Analyse Survey"}
								<FontAwesomeIcon icon={faMicroscope} className="ml-1" />
							</SkeletonButton>
						</div>
					</fieldset>
				</form>
				<div>
					{formState.messages.success !== "" && (
						<Alert
							title="Survey Created"
							message={formState.messages.success}
						/>
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
					{formState.analysis.error !== "" && (
						<Alert
							type="error"
							title="Survey Analysis Failed"
							message={formState.analysis.error}
						/>
					)}
					{formState.analysis.completed &&
						formState.analysis.warnings.length === 0 && (
							<Alert
								title="The Fun Police Report"
								message="No questionable survey decisions detected."
							/>
						)}
					{formState.analysis.warnings.map((warning) => (
						<Alert
							key={warning.code}
							type="warning"
							title={warning.title}
							message={warning.message}
						/>
					))}
				</div>
			</div>
		</SkeletonTheme>
	);
};

export default CreateSurvey;
