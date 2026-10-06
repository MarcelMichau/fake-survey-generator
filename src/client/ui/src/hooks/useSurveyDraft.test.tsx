import { act, StrictMode } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { mockApiClient } from "../test/mock-api-client";
import { render } from "../test/test-utils";
import { useApiClient } from "./useApiClient";
import { useSurveyDraft } from "./useSurveyDraft";

vi.mock("./useApiClient");

type Draft = ReturnType<typeof useSurveyDraft>;
type Settings = Parameters<typeof useSurveyDraft>[0];

const warning = {
	code: "question.leading",
	title: "Survey Has Already Voted",
	message: "A leading question",
	probability: 0.9,
};

const analysisResponse = {
	ok: true,
	status: 200,
	json: async () => ({ warnings: [warning] }),
};

const creationResponse = {
	ok: true,
	status: 201,
	json: async () => ({ id: 123, topic: "Tabs or spaces?" }),
};

async function renderDraft(settings: Settings = { loading: false }) {
	let draft: Draft;
	function Probe() {
		draft = useSurveyDraft(settings);
		return null;
	}
	await render(
		<StrictMode>
			<Probe />
		</StrictMode>,
	);
	return {
		get draft() {
			return draft;
		},
	};
}

function fillDraft(draft: Draft) {
	draft.updateSurveyField("topic", "Tabs or spaces?");
	draft.updateSurveyField("respondentType", "Developers");
	draft.updateSurveyField("numberOfRespondents", 100);
	draft.updateOption(1, { optionText: "Tabs", preferredNumberOfVotes: 5 });
	draft.addOption();
	draft.updateOption(2, { optionText: "Spaces" });
}

const edits: [string, (draft: Draft) => void][] = [
	["topic", (draft) => draft.updateSurveyField("topic", "Tea or coffee?")],
	[
		"audience",
		(draft) => draft.updateSurveyField("respondentType", "Designers"),
	],
	[
		"respondent count",
		(draft) => draft.updateSurveyField("numberOfRespondents", 200),
	],
	["option text", (draft) => draft.updateOption(1, { optionText: "Tea" })],
	[
		"preferred votes",
		(draft) => draft.updateOption(1, { preferredNumberOfVotes: 10 }),
	],
	["adding an option", (draft) => draft.addOption()],
	["removing an option", (draft) => draft.removeOption(2)],
];

describe("Survey draft workflow", () => {
	const transport = vi.fn();

	beforeEach(() => {
		vi.clearAllMocks();
		transport.mockResolvedValue(creationResponse);
		vi.mocked(useApiClient).mockReturnValue(mockApiClient(transport));
	});

	it("creates without analysis using current values and no draft-only option IDs", async () => {
		const onSurveyCreated = vi.fn();
		const probe = await renderDraft({ loading: false, onSurveyCreated });

		await act(async () => {
			fillDraft(probe.draft);
			await probe.draft.create();
		});

		expect(transport).toHaveBeenCalledTimes(1);
		expect(transport).toHaveBeenCalledWith("api/survey", {
			method: "POST",
			body: JSON.stringify({
				surveyTopic: "Tabs or spaces?",
				respondentType: "Developers",
				numberOfRespondents: 100,
				surveyOptions: [
					{ optionText: "Tabs", preferredNumberOfVotes: 5 },
					{ optionText: "Spaces", preferredNumberOfVotes: 0 },
				],
			}),
		});
		expect(onSurveyCreated).toHaveBeenCalledExactlyOnceWith(123);
		expect(probe.draft.survey.topic).toBe("Tabs or spaces?");
		expect(probe.draft.canAnalyze).toBe(false);
		expect(probe.draft.canCreate).toBe(true);
	});

	it("requires acknowledgement through the draft interface before creating analysed warnings", async () => {
		transport
			.mockResolvedValueOnce(analysisResponse)
			.mockResolvedValueOnce(creationResponse);
		const probe = await renderDraft({ loading: false, resetOnSuccess: false });

		await act(async () => {
			fillDraft(probe.draft);
			await probe.draft.analyze();
		});
		expect(probe.draft.analysis.warnings).toEqual([warning]);
		expect(probe.draft.canCreate).toBe(false);
		expect(probe.draft.canAnalyze).toBe(false);
		const command = JSON.parse(transport.mock.calls[0][1].body);
		expect(command).not.toHaveProperty("numberOfRespondents");
		expect(command.surveyOptions[0]).not.toHaveProperty("id");

		await act(async () => {
			await probe.draft.create();
			await probe.draft.analyze();
		});
		expect(transport).toHaveBeenCalledTimes(1);

		await act(async () => {
			probe.draft.acknowledgeWarnings(true);
			await probe.draft.create();
		});
		expect(transport).toHaveBeenCalledTimes(2);
		expect(probe.draft.analysis.warnings).toEqual([]);
		expect(probe.draft.messages.success).toContain("123");
	});

	it.each(edits)(
		"invalidates analysis and acknowledgement after editing %s",
		async (_, edit) => {
			transport.mockResolvedValue(analysisResponse);
			const probe = await renderDraft();
			await act(async () => {
				fillDraft(probe.draft);
				await probe.draft.analyze();
				probe.draft.acknowledgeWarnings(true);
			});
			expect(probe.draft.analysis.hasAcknowledgedWarnings).toBe(true);

			await act(async () => edit(probe.draft));

			expect(probe.draft.analysis.warnings).toEqual([]);
			expect(probe.draft.analysis.hasAcknowledgedWarnings).toBe(false);
			expect(probe.draft.analysis.completed).toBe(false);
			expect(probe.draft.canAnalyze).toBe(true);
			expect(probe.draft.canCreate).toBe(true);

			await act(async () => probe.draft.analyze());
			expect(probe.draft.canCreate).toBe(false);
		},
	);

	it.each(edits)(
		"starts a fresh draft after retained success when editing %s",
		async (_, edit) => {
			const probe = await renderDraft({
				loading: false,
				resetOnSuccess: false,
			});
			await act(async () => {
				fillDraft(probe.draft);
				await probe.draft.create();
			});
			expect(probe.draft.messages.success).toContain("123");
			expect(probe.draft.canAnalyze).toBe(false);

			await act(async () => edit(probe.draft));

			expect(probe.draft.messages.success).toBe("");
			expect(probe.draft.canAnalyze).toBe(true);
		},
	);

	it("keeps unchanged values submitted and option identity stable across edits", async () => {
		const probe = await renderDraft({ loading: false, resetOnSuccess: false });
		await act(async () => {
			fillDraft(probe.draft);
			await probe.draft.create();
			probe.draft.updateSurveyField("topic", "Tabs or spaces?");
			probe.draft.updateOption(1, { optionText: "Tabs" });
			probe.draft.removeOption(1);
			probe.draft.removeOption(99);
		});
		expect(probe.draft.canAnalyze).toBe(false);
		expect(probe.draft.messages.success).toContain("123");

		await act(async () => {
			probe.draft.removeOption(2);
			probe.draft.addOption();
		});
		expect(probe.draft.survey.options.map((option) => option.id)).toEqual([
			1, 3,
		]);
	});

	it.each(["analysis", "creation"] as const)(
		"blocks same-turn edits and competing actions during pending %s",
		async (operation) => {
			let completeResponse: (
				response: typeof creationResponse | typeof analysisResponse,
			) => void = () => {
				throw new Error("The transport request has not started");
			};
			const response = new Promise<
				typeof creationResponse | typeof analysisResponse
			>((resolve) => {
				completeResponse = resolve;
			});
			transport.mockImplementation(() => response);
			const onSurveyCreated = vi.fn();
			const probe = await renderDraft({ loading: false, onSurveyCreated });
			await act(async () => fillDraft(probe.draft));
			const before = probe.draft.survey;
			let pending: Promise<void>;

			await act(async () => {
				pending =
					operation === "analysis"
						? probe.draft.analyze()
						: probe.draft.create();
				for (const [, edit] of edits) edit(probe.draft);
				probe.draft.acknowledgeWarnings(true);
				await probe.draft.create();
				await probe.draft.analyze();
			});
			await expect.poll(() => transport.mock.calls.length).toBe(1);
			expect(probe.draft.survey).toEqual(before);
			expect(probe.draft.canEdit).toBe(false);
			expect(probe.draft.canCreate).toBe(false);
			expect(probe.draft.canAnalyze).toBe(false);
			expect(probe.draft.isAnalyzing).toBe(operation === "analysis");
			expect(probe.draft.isCreating).toBe(operation === "creation");

			await act(async () => {
				completeResponse(
					operation === "analysis" ? analysisResponse : creationResponse,
				);
				await pending;
			});
			expect(probe.draft.canEdit).toBe(true);
			expect(onSurveyCreated).toHaveBeenCalledTimes(
				operation === "creation" ? 1 : 0,
			);
		},
	);

	it("blocks all draft actions while externally loading", async () => {
		const probe = await renderDraft({ loading: true });
		await act(async () => {
			fillDraft(probe.draft);
			await probe.draft.analyze();
			await probe.draft.create();
		});
		expect(transport).not.toHaveBeenCalled();
		expect(probe.draft.survey.options).toHaveLength(1);
		expect(probe.draft.survey.topic).toBe("");
		expect(probe.draft.canEdit).toBe(false);
	});

	it.each(["analysis", "creation"] as const)(
		"retains values after failed %s and permits explicit retry without automatic requests",
		async (operation) => {
			transport.mockResolvedValue({ ok: false, status: 500 });
			const probe = await renderDraft({
				loading: false,
				resetOnSuccess: false,
			});
			await act(async () => {
				fillDraft(probe.draft);
				if (operation === "analysis") await probe.draft.analyze();
				else await probe.draft.create();
			});
			expect(transport).toHaveBeenCalledTimes(1);
			expect(probe.draft.survey.topic).toBe("Tabs or spaces?");
			expect(probe.draft.canEdit).toBe(true);
			expect(probe.draft.canAnalyze).toBe(true);
			expect(probe.draft.canCreate).toBe(true);
			expect(
				operation === "analysis"
					? probe.draft.analysis.error
					: probe.draft.messages.error,
			).not.toBe("");

			transport.mockResolvedValue(
				operation === "analysis" ? analysisResponse : creationResponse,
			);
			await act(async () => {
				if (operation === "analysis") await probe.draft.analyze();
				else await probe.draft.create();
			});
			expect(transport).toHaveBeenCalledTimes(2);
			expect(probe.draft.analysis.error).toBe("");
			expect(probe.draft.messages.error).toBe("");
		},
	);

	it("permits creation after clean analysis without acknowledgement", async () => {
		transport
			.mockResolvedValueOnce({
				ok: true,
				status: 200,
				json: async () => ({ warnings: [] }),
			})
			.mockResolvedValueOnce(creationResponse);
		const probe = await renderDraft();
		await act(async () => probe.draft.analyze());
		expect(probe.draft.analysis.completed).toBe(true);
		expect(probe.draft.canCreate).toBe(true);
		await act(async () => probe.draft.create());
		expect(transport).toHaveBeenCalledTimes(2);
	});

	it("keeps creation blocked after acknowledgement is revoked", async () => {
		transport.mockResolvedValue(analysisResponse);
		const probe = await renderDraft();
		await act(async () => {
			await probe.draft.analyze();
			probe.draft.acknowledgeWarnings(true);
			probe.draft.acknowledgeWarnings(false);
			await probe.draft.create();
		});
		expect(probe.draft.canCreate).toBe(false);
		expect(transport).toHaveBeenCalledTimes(1);
	});

	it("maps analysis validation errors and allows retry", async () => {
		transport.mockResolvedValue({
			ok: false,
			status: 422,
			json: async () => ({ surveyTopic: ["Topic is required"] }),
		});
		const probe = await renderDraft();
		await act(async () => probe.draft.analyze());
		expect(probe.draft.analysis.error).toBe("Topic is required");
		expect(probe.draft.canAnalyze).toBe(true);
	});

	it("retains warning acknowledgement when creation fails and maps validation outcomes", async () => {
		transport.mockResolvedValueOnce(analysisResponse).mockResolvedValue({
			ok: false,
			status: 422,
			json: async () => ({ surveyTopic: ["Topic is required"] }),
		});
		const probe = await renderDraft();
		await act(async () => {
			await probe.draft.analyze();
			probe.draft.acknowledgeWarnings(true);
			await probe.draft.create();
		});
		expect(probe.draft.messages.validationErrors).toEqual([
			"Topic is required",
		]);
		expect(probe.draft.messages.error).toBe("");
		expect(probe.draft.analysis.warnings).toEqual([warning]);
		expect(probe.draft.analysis.hasAcknowledgedWarnings).toBe(true);
		expect(probe.draft.canCreate).toBe(true);
	});

	it.each([
		["no callback", undefined, undefined, true],
		["callback", vi.fn(), undefined, false],
		["callback and explicit reset", vi.fn(), true, true],
		["callback and explicit retain", vi.fn(), false, false],
		["no callback and explicit retain", undefined, false, false],
	] as const)(
		"preserves success policy with %s",
		async (_, onSurveyCreated, resetOnSuccess, resets) => {
			const probe = await renderDraft({
				loading: false,
				onSurveyCreated,
				resetOnSuccess,
			});
			await act(async () => {
				fillDraft(probe.draft);
				await probe.draft.create();
			});
			expect(probe.draft.survey.topic).toBe(resets ? "" : "Tabs or spaces?");
			expect(probe.draft.survey.options).toHaveLength(resets ? 1 : 2);
			expect(probe.draft.canAnalyze).toBe(resets);
			expect(probe.draft.messages.success).toContain("123");
			if (onSurveyCreated)
				expect(onSurveyCreated).toHaveBeenCalledExactlyOnceWith(123);
			if (resets) {
				await act(async () => probe.draft.addOption());
				expect(probe.draft.survey.options.map((option) => option.id)).toEqual([
					1, 2,
				]);
			}
		},
	);
});
