import {
	faMicroscope,
	faMinus,
	faPaperPlane,
	faPlus,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import type React from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import { useSurveyDraft } from "../hooks/useSurveyDraft";
import Alert from "./Alert";
import Button from "./Button";
import Field from "./Field";
import SkeletonButton from "./SkeletonButton";

type CreateSurveyProps = {
	loading: boolean;
	onSurveyCreated?: (surveyId: number) => void;
	resetOnSuccess?: boolean;
};

const CreateSurvey = ({
	loading,
	onSurveyCreated,
	resetOnSuccess,
}: CreateSurveyProps): React.ReactElement => {
	const draft = useSurveyDraft({ loading, onSurveyCreated, resetOnSuccess });

	const onSubmit = async (
		e: React.SubmitEvent<HTMLFormElement>,
	): Promise<void> => {
		e.preventDefault();
		await draft.create();
	};

	return (
		<SkeletonTheme baseColor="#30353a" highlightColor="#c7ff18">
			<div className="brutal-panel h-full">
				<h2 className="display-title text-4xl lg:text-5xl mb-5">
					{loading ? <Skeleton /> : <span>Create Survey</span>}
				</h2>
				<form
					onSubmit={onSubmit}
					aria-busy={draft.isAnalyzing || draft.isCreating}
				>
					<fieldset disabled={!draft.canEdit}>
						<Field
							label="Target Audience (Respondent Type)"
							value={draft.survey.respondentType}
							onChange={(value) =>
								draft.updateSurveyField("respondentType", value)
							}
							loading={loading}
							placeholder="Pragmatic Developers"
						/>
						<Field
							label="Question (Survey Topic)"
							value={draft.survey.topic}
							onChange={(value) => draft.updateSurveyField("topic", value)}
							loading={loading}
							placeholder="Do you prefer tabs or spaces?"
						/>
						<Field
							label="Number of Respondents"
							value={draft.survey.numberOfRespondents}
							onChange={(value) =>
								draft.updateSurveyField(
									"numberOfRespondents",
									Number.isNaN(Number(value))
										? draft.survey.numberOfRespondents
										: Number(value),
								)
							}
							loading={loading}
						/>
						<span className="ui-label">
							{loading ? <Skeleton /> : <span>Options</span>}
						</span>
						{draft.survey.options.map((option, index) => {
							const optionNumber = index + 1;

							return (
								<div key={option.id}>
									<Field
										label={`#${optionNumber}`}
										value={option.optionText}
										onChange={(value) =>
											draft.updateOption(option.id, { optionText: value })
										}
										loading={loading}
										placeholder={
											index === 0 ? "Most definitely tabs" : "Some other option"
										}
									>
										{index > 0 && (
											<Button
												actionType="destructive"
												onClick={() => draft.removeOption(option.id)}
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
											max={draft.survey.numberOfRespondents}
											value={option.preferredNumberOfVotes}
											onChange={(e) => {
												const value = Number.parseInt(e.target.value, 10);
												draft.updateOption(option.id, {
													preferredNumberOfVotes: Number.isNaN(value)
														? 0
														: value,
												});
											}}
											disabled={loading}
											className="brutal-input max-w-40"
										/>
										<p className="text-gray-300 text-xs mt-2">
											{loading ? (
												<Skeleton width={200} />
											) : (
												`Set to 0 for random distribution or specify the desired number of votes (max: ${draft.survey.numberOfRespondents})`
											)}
										</p>
									</div>
								</div>
							);
						})}
						<div className="my-2">
							<SkeletonButton
								onClick={draft.addOption}
								loading={loading}
								actionType="secondary"
							>
								Add Option <FontAwesomeIcon icon={faPlus} className="ml-1" />
							</SkeletonButton>
						</div>
						<div className="mt-6 border-t-2 border-white pt-4">
							{draft.analysis.warnings.length > 0 && (
								<label className="my-3 flex items-start gap-3 text-sm text-paper">
									<input
										type="checkbox"
										checked={draft.analysis.hasAcknowledgedWarnings}
										onChange={(event) =>
											draft.acknowledgeWarnings(event.target.checked)
										}
										disabled={!draft.canEdit}
									/>
									I confirm that I want to create this survey with bad data
									&amp; that I feel bad about it
								</label>
							)}
							<div className="my-2">
								<SkeletonButton
									type="submit"
									loading={loading}
									disabled={!draft.canCreate}
								>
									{draft.analysis.warnings.length > 0
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
								onClick={() => void draft.analyze()}
								loading={loading}
								disabled={!draft.canAnalyze}
								actionType="secondary"
							>
								{draft.isAnalyzing ? "Analysing..." : "Analyse Survey"}
								<FontAwesomeIcon icon={faMicroscope} className="ml-1" />
							</SkeletonButton>
						</div>
					</fieldset>
				</form>
				<div>
					{draft.messages.success !== "" && (
						<Alert title="Survey Created" message={draft.messages.success} />
					)}
					{draft.messages.error !== "" && (
						<Alert
							type="error"
							title="Oh no! Something did not go as planned."
							message={draft.messages.error}
						/>
					)}
					{draft.messages.validationErrors.map((error, index) => (
						<Alert
							// biome-ignore lint/suspicious/noArrayIndexKey: no unique identifier available
							key={index}
							type="error"
							title="Validation Error"
							message={error}
						/>
					))}
					{draft.analysis.error !== "" && (
						<Alert
							type="error"
							title="Survey Analysis Failed"
							message={draft.analysis.error}
						/>
					)}
					{draft.analysis.completed && draft.analysis.warnings.length === 0 && (
						<Alert
							title="The Fun Police Report"
							message="No questionable survey decisions detected."
						/>
					)}
					{draft.analysis.warnings.map((warning) => (
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
