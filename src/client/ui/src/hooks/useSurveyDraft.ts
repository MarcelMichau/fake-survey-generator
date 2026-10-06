import { useRef, useState } from "react";
import { ApiError } from "../api/ApiError";
import type { CreateSurveyCommand, SurveyAnalysisWarningModel } from "../types";
import { useAnalyzeSurvey, useCreateSurvey } from "./useSurveys";

type DraftOption = {
	id: number;
	optionText: string;
	preferredNumberOfVotes: number;
};

type DraftSurvey = {
	respondentType: string;
	topic: string;
	numberOfRespondents: number;
	options: DraftOption[];
};

type DraftState = {
	survey: DraftSurvey;
	messages: {
		success: string;
		error: string;
		validationErrors: string[];
	};
	analysis: {
		completed: boolean;
		warnings: SurveyAnalysisWarningModel[];
		hasAcknowledgedWarnings: boolean;
		error: string;
	};
	phase: "editing" | "analyzing" | "creating";
	isCreated: boolean;
	nextOptionId: number;
};

type DraftSettings = {
	loading: boolean;
	onSurveyCreated?: (surveyId: number) => void;
	resetOnSuccess?: boolean;
};

function emptyMessages(): DraftState["messages"] {
	return { success: "", error: "", validationErrors: [] };
}

function emptyAnalysis(): DraftState["analysis"] {
	return {
		completed: false,
		warnings: [],
		hasAcknowledgedWarnings: false,
		error: "",
	};
}

function initialDraft(): DraftState {
	return {
		survey: {
			respondentType: "",
			topic: "",
			numberOfRespondents: 0,
			options: [{ id: 1, optionText: "", preferredNumberOfVotes: 0 }],
		},
		messages: emptyMessages(),
		analysis: emptyAnalysis(),
		phase: "editing",
		isCreated: false,
		nextOptionId: 2,
	};
}

function buildCommand(survey: DraftSurvey): CreateSurveyCommand {
	return {
		surveyTopic: survey.topic,
		respondentType: survey.respondentType,
		numberOfRespondents: survey.numberOfRespondents,
		surveyOptions: survey.options.map(
			({ optionText, preferredNumberOfVotes }) => ({
				optionText,
				preferredNumberOfVotes,
			}),
		),
	};
}

/** Owns draft transitions and their outcomes; transport and cache policy stay in useSurveys. */
export function useSurveyDraft({
	loading,
	onSurveyCreated,
	resetOnSuccess = !onSurveyCreated,
}: DraftSettings) {
	const creation = useCreateSurvey();
	const analysis = useAnalyzeSurvey();
	const [state, setState] = useState(initialDraft);
	// Actions must see preceding transitions immediately, before React renders again.
	// This also prevents same-turn submissions and edits from bypassing pending work.
	const current = useRef(state);

	function commit(next: DraftState) {
		current.current = next;
		setState(next);
	}

	function canEdit(draft: DraftState) {
		return !loading && draft.phase === "editing";
	}

	function canCreate(draft: DraftState) {
		return (
			canEdit(draft) &&
			(draft.analysis.warnings.length === 0 ||
				draft.analysis.hasAcknowledgedWarnings)
		);
	}

	function canAnalyze(draft: DraftState) {
		return canEdit(draft) && !draft.analysis.completed && !draft.isCreated;
	}

	function edit(change: (draft: DraftState) => DraftState) {
		const draft = current.current;
		if (!canEdit(draft)) return;
		const next = change(draft);
		if (next === draft) return;
		commit({
			...next,
			messages: { ...next.messages, success: "" },
			analysis: emptyAnalysis(),
			isCreated: false,
		});
	}

	function updateSurveyField<K extends Exclude<keyof DraftSurvey, "options">>(
		key: K,
		value: DraftSurvey[K],
	) {
		edit((draft) =>
			draft.survey[key] === value
				? draft
				: { ...draft, survey: { ...draft.survey, [key]: value } },
		);
	}

	function updateOption(id: number, changes: Partial<Omit<DraftOption, "id">>) {
		edit((draft) => {
			const option = draft.survey.options.find((option) => option.id === id);
			if (!option) return draft;
			const updated = { ...option, ...changes };
			if (
				updated.optionText === option.optionText &&
				updated.preferredNumberOfVotes === option.preferredNumberOfVotes
			)
				return draft;
			return {
				...draft,
				survey: {
					...draft.survey,
					options: draft.survey.options.map((option) =>
						option.id === id ? updated : option,
					),
				},
			};
		});
	}

	function addOption() {
		edit((draft) => ({
			...draft,
			nextOptionId: draft.nextOptionId + 1,
			survey: {
				...draft.survey,
				options: [
					...draft.survey.options,
					{
						id: draft.nextOptionId,
						optionText: "",
						preferredNumberOfVotes: 0,
					},
				],
			},
		}));
	}

	function removeOption(id: number) {
		edit((draft) => {
			// The first option remains available, just as in the rendered form.
			if (!draft.survey.options.slice(1).some((option) => option.id === id))
				return draft;
			return {
				...draft,
				survey: {
					...draft.survey,
					options: draft.survey.options.filter((option) => option.id !== id),
				},
			};
		});
	}

	function acknowledgeWarnings(acknowledged: boolean) {
		const draft = current.current;
		if (!canEdit(draft) || draft.analysis.warnings.length === 0) return;
		commit({
			...draft,
			analysis: { ...draft.analysis, hasAcknowledgedWarnings: acknowledged },
		});
	}

	async function create() {
		const draft = current.current;
		if (!canCreate(draft)) return;
		commit({ ...draft, messages: emptyMessages(), phase: "creating" });
		try {
			const survey = await creation.mutateAsync(buildCommand(draft.survey));
			commit({
				...(resetOnSuccess ? initialDraft() : current.current),
				phase: "editing",
				isCreated: !resetOnSuccess,
				messages: {
					...emptyMessages(),
					success: `Survey created with ID: ${survey.id}. Get the survey to see the outcome.`,
				},
				analysis: emptyAnalysis(),
			});
			onSurveyCreated?.(survey.id);
		} catch (error) {
			commit({
				...current.current,
				phase: "editing",
				messages: {
					success: "",
					error:
						error instanceof ApiError
							? error.validationErrors.length > 0
								? ""
								: error.message
							: "An unexpected error occurred",
					validationErrors:
						error instanceof ApiError ? error.validationErrors : [],
				},
			});
		}
	}

	async function analyze() {
		const draft = current.current;
		if (!canAnalyze(draft)) return;
		commit({
			...draft,
			phase: "analyzing",
			messages: emptyMessages(),
			analysis: emptyAnalysis(),
		});
		const { numberOfRespondents: _respondents, ...command } = buildCommand(
			draft.survey,
		);
		try {
			const result = await analysis.mutateAsync(command);
			commit({
				...current.current,
				phase: "editing",
				analysis: {
					...emptyAnalysis(),
					completed: true,
					warnings: result.warnings,
				},
			});
		} catch (error) {
			commit({
				...current.current,
				phase: "editing",
				analysis: {
					...emptyAnalysis(),
					error:
						error instanceof ApiError
							? error.validationErrors.join(" ") || error.message
							: "An unexpected error occurred while analyzing the survey.",
				},
			});
		}
	}

	return {
		survey: state.survey,
		messages: state.messages,
		analysis: state.analysis,
		isAnalyzing: state.phase === "analyzing",
		isCreating: state.phase === "creating",
		canEdit: canEdit(state),
		canCreate: canCreate(state),
		canAnalyze: canAnalyze(state),
		updateSurveyField,
		updateOption,
		addOption,
		removeOption,
		acknowledgeWarnings,
		create,
		analyze,
	};
}
