import {
	faMicroscope,
	faPaperPlane,
	faPlus,
	faXmark,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import type React from "react";
import {
	useEffect,
	useId,
	useLayoutEffect,
	useMemo,
	useRef,
	useState,
} from "react";
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
}: CreateSurveyProps) => {
	const draft = useSurveyDraft({ loading, onSurveyCreated, resetOnSuccess });
	const id = useId();
	const formRef = useRef<HTMLFormElement>(null);
	const summaryRef = useRef<HTMLElement>(null);
	const addRef = useRef<HTMLButtonElement>(null);
	const optionInputs = useRef(new Map<number, HTMLInputElement>());
	const pendingFocus = useRef<number | "new" | null>(null);
	const [optionLimitError, setOptionLimitError] = useState("");
	const fieldErrors = useMemo(
		() => ({
			...draft.messages.fieldErrors,
			...draft.analysis.fieldErrors,
			...(optionLimitError ? { SurveyOptions: [optionLimitError] } : {}),
		}),
		[draft.messages.fieldErrors, draft.analysis.fieldErrors, optionLimitError],
	);

	useEffect(() => {
		if (Object.keys(fieldErrors).length) summaryRef.current?.focus();
	}, [fieldErrors]);

	useLayoutEffect(() => {
		const target = pendingFocus.current;
		if (target === null) return;
		const optionId =
			target === "new" ? draft.survey.options.at(-1)?.id : target;
		const input =
			optionId === undefined ? undefined : optionInputs.current.get(optionId);
		(input ?? addRef.current)?.focus();
		pendingFocus.current = null;
	}, [draft.survey.options]);

	const errorFor = (field: string) =>
		Object.entries(fieldErrors)
			.find(([key]) => key.toLowerCase() === field.toLowerCase())?.[1]
			.join(" ");

	const targetFor = (
		field: string,
	): { id: string; label: string } | undefined => {
		switch (field.toLowerCase()) {
			case "surveytopic":
				return { id: `${id}-topic`, label: "Question" };
			case "respondenttype":
				return { id: `${id}-audience`, label: "Target Audience" };
			case "numberofrespondents":
				return { id: `${id}-respondents`, label: "Number of Respondents" };
			case "surveyoptions":
				return { id: `${id}-options`, label: "Options" };
		}
		const match =
			/^surveyoptions\[(\d+)\]\.(optiontext|preferrednumberofvotes)$/i.exec(
				field,
			);
		const option = match && draft.survey.options[Number(match[1])];
		if (!match || !option) return;
		const votes = match[2].toLowerCase() === "preferrednumberofvotes";
		return {
			id: `${id}-${votes ? "votes" : "option"}-${option.id}`,
			label: `${votes ? "Preferred Votes for" : "Option"} #${Number(match[1]) + 1}`,
		};
	};

	const onSubmit = async (event: React.SubmitEvent<HTMLFormElement>) => {
		event.preventDefault();
		if (!draft.canEdit) return;
		setOptionLimitError("");
		const submitter = event.nativeEvent.submitter;
		if (
			submitter instanceof HTMLButtonElement &&
			submitter.value === "analyze"
		) {
			// Analysis omits respondent count and vote allocation: validate its text inputs only.
			const inputs = Array.from(
				formRef.current?.querySelectorAll<HTMLInputElement>(
					"input[data-analysis-field]",
				) ?? [],
			);
			const invalid = inputs.filter((input) => !input.checkValidity());
			if (invalid.length) {
				invalid[0].reportValidity();
				return;
			}
			if (draft.survey.options.length > 20) {
				setOptionLimitError(
					"Analyse up to 20 options at once. You can still create a survey with up to 100 options.",
				);
				return;
			}
			await draft.analyze();
		} else await draft.create();
	};

	return (
		<SkeletonTheme baseColor="#30353a" highlightColor="#c7ff18">
			<div className="brutal-panel h-full">
				<h2
					id={`${id}-title`}
					className="display-title mb-6 text-4xl lg:text-5xl"
				>
					{loading ? <Skeleton /> : <span>Create Survey</span>}
				</h2>
				<form
					ref={formRef}
					onSubmit={onSubmit}
					aria-labelledby={`${id}-title`}
					aria-busy={draft.isAnalyzing || draft.isCreating}
				>
					<fieldset disabled={!draft.canEdit}>
						<legend className="sr-only">Survey details</legend>
						<Field
							id={`${id}-audience`}
							name="RespondentType"
							data-analysis-field
							label="Target Audience (Respondent Type)"
							required
							maxLength={250}
							pattern=".*\S.*"
							value={draft.survey.respondentType}
							onChange={(value) =>
								draft.updateSurveyField("respondentType", value)
							}
							error={errorFor("RespondentType")}
							loading={loading}
							placeholder="Pragmatic Developers"
						/>
						<Field
							id={`${id}-topic`}
							name="SurveyTopic"
							data-analysis-field
							label="Question (Survey Topic)"
							required
							maxLength={250}
							pattern=".*\S.*"
							value={draft.survey.topic}
							onChange={(value) => draft.updateSurveyField("topic", value)}
							error={errorFor("SurveyTopic")}
							loading={loading}
							placeholder="Do you prefer tabs or spaces?"
						/>
						<Field
							id={`${id}-respondents`}
							name="NumberOfRespondents"
							label="Number of Respondents"
							type="number"
							inputMode="numeric"
							required
							min={1}
							max={1_000_000}
							step={1}
							hint="Enter a whole number from 1 to 1,000,000. Not needed for analysis."
							value={draft.survey.numberOfRespondents}
							onChange={(value) =>
								draft.updateSurveyField(
									"numberOfRespondents",
									value === "" ? "" : Number(value),
								)
							}
							error={errorFor("NumberOfRespondents")}
							loading={loading}
						/>
						<fieldset className="mt-7">
							<legend
								id={`${id}-options`}
								tabIndex={-1}
								className="ui-label mb-3 border-b-2 border-line pb-2 text-base!"
							>
								{loading ? <Skeleton /> : "Options"}
							</legend>
							<p id={`${id}-options-hint`} className="ui-hint mb-3">
								Up to 100 options for creation; up to 20 for analysis. Option
								text is required.
							</p>
							<div className="space-y-3">
								{draft.survey.options.map((option, index) => (
									<div
										key={option.id}
										className="flex flex-wrap items-end gap-3 border-2 border-line border-l-lime border-l-[6px] bg-ink/40 p-3 sm:p-4"
									>
										<Field
											id={`${id}-option-${option.id}`}
											name={`SurveyOptions[${index}].OptionText`}
											data-analysis-field
											ref={(input) => {
												if (input) optionInputs.current.set(option.id, input);
												else optionInputs.current.delete(option.id);
											}}
											label={`#${index + 1}`}
											required
											maxLength={250}
											pattern=".*\S.*"
											value={option.optionText}
											onChange={(value) =>
												draft.updateOption(option.id, { optionText: value })
											}
											error={errorFor(`SurveyOptions[${index}].OptionText`)}
											loading={loading}
											className="min-w-48 flex-1"
											placeholder={
												index === 0
													? "Most definitely tabs"
													: "Some other option"
											}
										/>
										<Field
											id={`${id}-votes-${option.id}`}
											name={`SurveyOptions[${index}].PreferredNumberOfVotes`}
											label={`Preferred Votes for #${index + 1}`}
											type="number"
											inputMode="numeric"
											min={0}
											max={draft.survey.numberOfRespondents || undefined}
											step={1}
											value={option.preferredNumberOfVotes}
											onChange={(value) =>
												draft.updateOption(option.id, {
													preferredNumberOfVotes:
														value === "" ? "" : Number(value),
												})
											}
											error={errorFor(
												`SurveyOptions[${index}].PreferredNumberOfVotes`,
											)}
											loading={loading}
											className={index > 0 ? "w-48" : "w-32"}
											aria-describedby={`${id}-votes-hint`}
										>
											{index > 0 && (
												<button
													type="button"
													aria-label={`Remove #${index + 1}`}
													onClick={() => {
														pendingFocus.current =
															draft.survey.options[index + 1]?.id ??
															draft.survey.options[index - 1]?.id ??
															null;
														setOptionLimitError("");
														draft.removeOption(option.id);
													}}
													className="brutal-icon-button min-h-11! min-w-11!"
												>
													<FontAwesomeIcon icon={faXmark} />
												</button>
											)}
										</Field>
									</div>
								))}
							</div>
							<p id={`${id}-votes-hint`} className="ui-hint mt-3">
								{loading ? (
									<Skeleton width={200} />
								) : (
									`Set preferred votes to 0 for a random distribution, or specify the desired number of votes (max: ${draft.survey.numberOfRespondents || "number of respondents"}).`
								)}
							</p>
							<div className="mt-4">
								<SkeletonButton
									ref={addRef}
									loading={loading}
									disabled={draft.survey.options.length >= 100}
									actionType="secondary"
									additionalClasses={["text-base!"]}
									onClick={() => {
										pendingFocus.current = "new";
										draft.addOption();
									}}
								>
									Add Option <FontAwesomeIcon icon={faPlus} />
								</SkeletonButton>
							</div>
						</fieldset>
						<div className="mt-7 border-t-2 border-line pt-6">
							{draft.analysis.warnings.length > 0 && (
								<label
									htmlFor={`${id}-acknowledgement`}
									className="mb-5 flex cursor-pointer items-start gap-3 text-sm text-paper"
								>
									<input
										id={`${id}-acknowledgement`}
										name="acknowledgement"
										type="checkbox"
										className="mt-0.5 size-4 shrink-0"
										checked={draft.analysis.hasAcknowledgedWarnings}
										onChange={(event) =>
											draft.acknowledgeWarnings(event.target.checked)
										}
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
									type="submit"
									name="intent"
									value="analyze"
									formNoValidate
									loading={loading}
									disabled={!draft.canAnalyze}
									actionType="secondary"
								>
									{draft.isAnalyzing ? "Analysing..." : "Analyse Survey"}{" "}
									<FontAwesomeIcon icon={faMicroscope} />
								</SkeletonButton>
							</div>
							<p className="ui-hint mt-4">
								Analyse provided survey information for potential issues.
							</p>
						</div>
					</fieldset>
				</form>
				{Object.keys(fieldErrors).length > 0 && (
					<section
						ref={summaryRef}
						tabIndex={-1}
						aria-labelledby={`${id}-errors-title`}
						className="my-4 border-2 border-[#ff8e83] bg-[#3b2525] p-4"
					>
						<h3 id={`${id}-errors-title`} className="ui-label">
							Check survey details
						</h3>
						<ul className="list-disc space-y-2 pl-5">
							{Object.entries(fieldErrors).flatMap(([field, messages]) =>
								[...new Set(messages)].map((message) => {
									const target = targetFor(field);
									return (
										<li key={`${field}:${message}`}>
											{target ? (
												<a
													className="underline"
													href={`#${target.id}`}
													onClick={(event) => {
														event.preventDefault();
														document.getElementById(target.id)?.focus();
													}}
												>
													{target.label}: {message}
												</a>
											) : (
												message
											)}
										</li>
									);
								}),
							)}
						</ul>
					</section>
				)}
				{draft.messages.success && (
					<Alert title="Survey Created" message={draft.messages.success} />
				)}
				{draft.messages.error && (
					<Alert
						type="error"
						title="Oh no! Something did not go as planned."
						message={draft.messages.error}
					/>
				)}
				{draft.analysis.error &&
					!Object.keys(draft.analysis.fieldErrors).length && (
						<Alert
							type="error"
							title="Survey Analysis Failed"
							message={draft.analysis.error}
						/>
					)}
				<p role="status" className="ui-hint mt-4">
					{draft.analysis.completed && draft.analysis.warnings.length > 0
						? `Analysis complete: ${draft.analysis.warnings.length} issues to review before creating the survey.`
						: ""}
				</p>
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
						announcement="off"
						title={warning.title}
						message={warning.message}
					/>
				))}
			</div>
		</SkeletonTheme>
	);
};

export default CreateSurvey;
