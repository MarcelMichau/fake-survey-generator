import {
	faMicroscope,
	faPaperPlane,
	faPlus,
	faXmark,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import type React from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import { useSurveyDraft } from "../hooks/useSurveyDraft";
import Alert from "./Alert";
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
				<h2 className="display-title mb-6 text-4xl lg:text-5xl">
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
						<span className="ui-label mt-7 mb-3 border-b-2 border-line pb-2 text-base!">
							{loading ? <Skeleton /> : <span>Options</span>}
						</span>
						<div className="space-y-3">
							{draft.survey.options.map((option, index) => {
								const optionNumber = index + 1;

								return (
									<div
										key={option.id}
										className="flex flex-wrap items-end gap-3 border-2 border-line border-l-lime border-l-[6px] bg-ink/40 p-3 sm:p-4"
									>
										<Field
											label={`#${optionNumber}`}
											value={option.optionText}
											onChange={(value) =>
												draft.updateOption(option.id, { optionText: value })
											}
											loading={loading}
											className="min-w-48 flex-1"
											placeholder={
												index === 0
													? "Most definitely tabs"
													: "Some other option"
											}
										/>
										<div className="w-32">
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
												className="brutal-input"
											/>
										</div>
										{index > 0 && (
											<button
												type="button"
												aria-label={`Remove #${optionNumber}`}
												title={`Remove #${optionNumber}`}
												onClick={() => draft.removeOption(option.id)}
												className="brutal-icon-button min-h-11! min-w-11!"
											>
												<FontAwesomeIcon icon={faXmark} />
											</button>
										)}
									</div>
								);
							})}
						</div>
						<p className="ui-hint mt-3">
							{loading ? (
								<Skeleton width={200} />
							) : (
								`Set preferred votes to 0 for a random distribution, or specify the desired number of votes (max: ${draft.survey.numberOfRespondents}).`
							)}
						</p>
						<div className="mt-4">
							<SkeletonButton
								onClick={draft.addOption}
								loading={loading}
								actionType="secondary"
								additionalClasses={["text-base!"]}
							>
								Add Option <FontAwesomeIcon icon={faPlus} />
							</SkeletonButton>
						</div>
						<div className="mt-7 border-t-2 border-line pt-6">
							{draft.analysis.warnings.length > 0 && (
								<label className="mb-5 flex cursor-pointer items-start gap-3 text-sm text-paper">
									<input
										type="checkbox"
										className="mt-0.5 size-4 shrink-0"
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
							<div className="flex flex-wrap gap-4">
								<SkeletonButton
									type="submit"
									loading={loading}
									disabled={!draft.canCreate}
								>
									{draft.analysis.warnings.length > 0
										? "Create Survey (Despite All The Issues Identified)"
										: "Create Survey"}{" "}
									<FontAwesomeIcon icon={faPaperPlane} />
								</SkeletonButton>
								<SkeletonButton
									onClick={() => void draft.analyze()}
									loading={loading}
									disabled={!draft.canAnalyze}
									actionType="secondary"
								>
									{draft.isAnalyzing ? "Analysing..." : "Analyse Survey"}
									<FontAwesomeIcon icon={faMicroscope} />
								</SkeletonButton>
							</div>
							<p className="ui-hint mt-4">
								Analyse provided survey information for potential issues.
							</p>
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
