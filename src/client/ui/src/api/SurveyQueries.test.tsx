import { useAuth0 } from "@auth0/auth0-react";
import { type QueryClient, useQueryClient } from "@tanstack/react-query";
import createClient from "openapi-fetch";
import { act, StrictMode, useState } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { render as browserRender } from "vitest-browser-react";
import { useApiClient } from "../hooks/useApiClient";
import { useSurveyFetch } from "../hooks/useSurveyFetch";
import {
	useCreateSurvey,
	useDeleteSurvey,
	useUserSurveys,
} from "../hooks/useSurveys";
import { useUserRegistration } from "../hooks/useUserRegistration";
import { render } from "../test/test-utils";
import type { SurveyModel } from "../types";
import { ApiQueryProvider } from "./ApiQueryProvider";
import type { paths } from "./generated";
import { registrationKey, useRegisteredSurveyMutation } from "./SurveySession";

vi.mock("../hooks/useApiClient");

const survey: SurveyModel = {
	id: 1,
	ownerId: 1,
	ownerExternalUserId: "test-user-id",
	topic: "Tabs or spaces?",
	respondentType: "Developers",
	numberOfRespondents: 100,
	isRigged: false,
	options: [],
	createdBy: null,
	createdOn: "2026-09-30T00:00:00Z",
	modifiedBy: null,
	modifiedOn: null,
};

function response(data: unknown, status = 200) {
	return new Response(status === 204 ? null : JSON.stringify(data), {
		status,
		headers: { "Content-Type": "application/json" },
	});
}

function Detail({ id }: { id: number | null }) {
	const query = useSurveyFetch(id);
	return (
		<div>
			<span>{query.survey?.topic ?? "No survey"}</span>
			<span>{query.error}</span>
			<button type="button" onClick={() => void query.refetch()}>
				Refresh detail
			</button>
		</div>
	);
}

describe("Survey query lifecycle", () => {
	beforeEach(() => vi.clearAllMocks());

	it("shares concurrent detail requests, including StrictMode replay", async () => {
		const fetch = vi.fn(async () => response(survey));
		vi.mocked(useApiClient).mockReturnValue(
			createClient<paths>({ baseUrl: window.location.origin, fetch }),
		);
		const screen = await render(
			<StrictMode>
				<Detail id={1} />
				<Detail id={1} />
			</StrictMode>,
		);
		await expect
			.element(screen.getByText(survey.topic).first())
			.toBeInTheDocument();
		expect(fetch).toHaveBeenCalledTimes(1);
	});

	it("aborts the old request when a different survey is selected", async () => {
		let previousSignal: AbortSignal | undefined;
		const fetch = vi.fn(async (request: Request) => {
			if (request.url.endsWith("/1")) {
				previousSignal = request.signal;
				return new Promise<Response>((_, reject) =>
					request.signal.addEventListener("abort", () =>
						reject(request.signal.reason),
					),
				);
			}
			return response({ ...survey, id: 2, topic: "New survey" });
		});
		vi.mocked(useApiClient).mockReturnValue(
			createClient<paths>({ baseUrl: window.location.origin, fetch }),
		);
		const screen = await render(<Detail id={1} />);
		await expect.poll(() => previousSignal).toBeDefined();
		await screen.rerender(<Detail id={2} />);
		await expect.element(screen.getByText("New survey")).toBeInTheDocument();
		expect(previousSignal?.aborted).toBe(true);
		await expect
			.element(screen.getByText(survey.topic))
			.not.toBeInTheDocument();
	});

	it("clears displayed stale data after a refresh fails", async () => {
		const fetch = vi
			.fn()
			.mockResolvedValueOnce(response(survey))
			.mockResolvedValue(response({}, 403));
		vi.mocked(useApiClient).mockReturnValue(
			createClient<paths>({ baseUrl: window.location.origin, fetch }),
		);
		const screen = await render(<Detail id={1} />);
		await expect.element(screen.getByText(survey.topic)).toBeInTheDocument();
		await screen.getByRole("button", { name: "Refresh detail" }).click();
		await expect
			.element(screen.getByText("Something did not go as planned"))
			.toBeInTheDocument();
		await expect
			.element(screen.getByText(survey.topic))
			.not.toBeInTheDocument();
		expect(fetch).toHaveBeenCalledTimes(2);
	});

	it("seeds created details and refreshes the existing list", async () => {
		let created = false;
		const fetch = vi.fn(async (request: Request) => {
			if (request.method === "POST") {
				created = true;
				return response(survey, 201);
			}
			if (request.url.endsWith("/user"))
				return response(created ? [survey] : []);
			throw new Error("Creation should seed the detail cache");
		});
		vi.mocked(useApiClient).mockReturnValue(
			createClient<paths>({ baseUrl: window.location.origin, fetch }),
		);
		function Create() {
			const list = useUserSurveys(true);
			const mutation = useCreateSurvey();
			const [id, setId] = useState<number | null>(null);
			return (
				<>
					<span>List count: {list.data?.length}</span>
					<button
						type="button"
						onClick={async () => {
							const result = await mutation.mutateAsync({
								surveyTopic: survey.topic,
								respondentType: "Developers",
								numberOfRespondents: 100,
								surveyOptions: [{ optionText: "Tabs" }],
							});
							setId(result.id);
						}}
					>
						Create
					</button>
					<Detail id={id} />
				</>
			);
		}
		const screen = await render(<Create />);
		await expect.element(screen.getByText("List count: 0")).toBeInTheDocument();
		await screen.getByRole("button", { name: "Create", exact: true }).click();
		await expect.element(screen.getByText(survey.topic)).toBeInTheDocument();
		await expect.element(screen.getByText("List count: 1")).toBeInTheDocument();
		expect(fetch).toHaveBeenCalledTimes(3);
	});

	it("deletes an active detail and refreshes its list without fetching the deleted record", async () => {
		let deleted = false;
		const fetch = vi.fn(async (request: Request) => {
			if (request.method === "DELETE") {
				deleted = true;
				return response(null, 204);
			}
			if (request.url.endsWith("/user"))
				return response(deleted ? [] : [survey]);
			return response(survey);
		});
		vi.mocked(useApiClient).mockReturnValue(
			createClient<paths>({ baseUrl: window.location.origin, fetch }),
		);
		function Delete() {
			const list = useUserSurveys(true);
			const mutation = useDeleteSurvey();
			return (
				<>
					<span>List count: {list.data?.length}</span>
					<Detail id={1} />
					<button type="button" onClick={() => mutation.mutate(1)}>
						Delete
					</button>
				</>
			);
		}
		const screen = await render(<Delete />);
		await expect.element(screen.getByText("List count: 1")).toBeInTheDocument();
		await expect.element(screen.getByText(survey.topic)).toBeInTheDocument();
		await screen.getByRole("button", { name: "Delete", exact: true }).click();
		await expect.element(screen.getByText("List count: 0")).toBeInTheDocument();
		await expect.element(screen.getByText("No survey")).toBeInTheDocument();
		expect(fetch).toHaveBeenCalledTimes(4);
	});

	it.each(["creation", "deletion"] as const)(
		"keeps accepted %s successful when cache synchronization rejects",
		async (operation) => {
			const fetch = vi.fn(async () =>
				operation === "creation" ? response(survey, 201) : response(null, 204),
			);
			vi.mocked(useApiClient).mockReturnValue(
				createClient<paths>({ baseUrl: window.location.origin, fetch }),
			);
			let workflow:
				| {
						cache: QueryClient;
						creation: ReturnType<typeof useCreateSurvey>;
						deletion: ReturnType<typeof useDeleteSurvey>;
				  }
				| undefined;
			function Probe() {
				workflow = {
					cache: useQueryClient(),
					creation: useCreateSurvey(),
					deletion: useDeleteSurvey(),
				};
				return null;
			}
			await render(<Probe />);
			if (!workflow) throw new Error("Workflow did not render");
			const { cache, creation, deletion } = workflow;
			const cacheError = new Error("Cache synchronization failed");
			const invalidate = vi
				.spyOn(cache, "invalidateQueries")
				.mockRejectedValue(cacheError);
			const report = vi.spyOn(console, "error").mockImplementation(() => {});
			const onSuccess = vi.fn();
			try {
				await act(async () => {
					if (operation === "creation") {
						await expect(
							creation.mutateAsync(
								{
									surveyTopic: survey.topic,
									respondentType: survey.respondentType,
									numberOfRespondents: 100,
									surveyOptions: [{ optionText: "Tabs" }],
								},
								{ onSuccess },
							),
						).resolves.toEqual(survey);
					} else {
						await expect(deletion.mutateAsync(1, { onSuccess })).resolves.toBe(
							1,
						);
					}
				});
				expect(
					operation === "creation"
						? workflow.creation.isSuccess
						: workflow.deletion.isSuccess,
				).toBe(true);
				expect(onSuccess).toHaveBeenCalledTimes(1);
				expect(fetch).toHaveBeenCalledTimes(1);
				expect(report).toHaveBeenCalledWith(
					"Survey mutation succeeded, but cache synchronization failed.",
					cacheError,
				);
			} finally {
				invalidate.mockRestore();
				report.mockRestore();
			}
		},
	);

	it.each([false, true])(
		"propagates readiness loss during cache synchronization (cache also rejects: %s)",
		async (cacheRejects) => {
			const fetch = vi.fn(async () => response(survey, 201));
			vi.mocked(useApiClient).mockReturnValue(
				createClient<paths>({ baseUrl: window.location.origin, fetch }),
			);
			let workflow:
				| { cache: QueryClient; creation: ReturnType<typeof useCreateSurvey> }
				| undefined;
			function Probe() {
				workflow = { cache: useQueryClient(), creation: useCreateSurvey() };
				return null;
			}
			await render(<Probe />);
			if (!workflow) throw new Error("Workflow did not render");
			const { cache, creation } = workflow;
			const invalidate = vi
				.spyOn(cache, "invalidateQueries")
				.mockImplementation(async () => {
					cache
						.getQueryCache()
						.find({ queryKey: registrationKey("test-user-id") })
						?.setState({ status: "error" });
					if (cacheRejects)
						throw new Error("Cache sync failed during readiness loss");
				});
			const report = vi.spyOn(console, "error").mockImplementation(() => {});
			const onSuccess = vi.fn();
			try {
				await act(async () => {
					await expect(
						creation.mutateAsync(
							{
								surveyTopic: survey.topic,
								respondentType: survey.respondentType,
								numberOfRespondents: 100,
								surveyOptions: [{ optionText: "Tabs" }],
							},
							{ onSuccess },
						),
					).rejects.toThrow("until user registration succeeds");
				});
				expect(onSuccess).not.toHaveBeenCalled();
				expect(report).not.toHaveBeenCalledWith(
					"Survey mutation succeeded, but cache synchronization failed.",
					expect.anything(),
				);
				expect(fetch).toHaveBeenCalledTimes(1);
			} finally {
				invalidate.mockRestore();
				report.mockRestore();
			}
		},
	);

	it("does not swallow unrelated success callback failures as cache failures", async () => {
		vi.mocked(useApiClient).mockReturnValue(
			createClient<paths>({ baseUrl: window.location.origin }),
		);
		const callbackError = new Error("Unrelated success callback failed");
		let mutation:
			| ReturnType<typeof useRegisteredSurveyMutation<number, void>>
			| undefined;
		function Probe() {
			mutation = useRegisteredSurveyMutation({
				mutationFn: async () => 1,
				onSuccess: () => {
					throw callbackError;
				},
			});
			return null;
		}
		await render(<Probe />);
		if (!mutation) throw new Error("Mutation did not render");
		const report = vi.spyOn(console, "error").mockImplementation(() => {});
		try {
			await act(async () => {
				await expect(mutation?.mutateAsync()).rejects.toBe(callbackError);
			});
			expect(report).not.toHaveBeenCalledWith(
				"Survey mutation succeeded, but cache synchronization failed.",
				expect.anything(),
			);
		} finally {
			report.mockRestore();
		}
	});

	it("starts with an empty cache after switching authenticated users", async () => {
		const auth = useAuth0();
		let detailRequests = 0;
		const fetch = vi.fn(async (request: Request) => {
			if (request.url.endsWith("/api/user/register"))
				return response({ id: 1 }, 201);
			detailRequests += 1;
			return response(
				detailRequests === 1
					? survey
					: { ...survey, topic: "Other user's survey" },
			);
		});
		vi.mocked(useApiClient).mockReturnValue(
			createClient<paths>({ baseUrl: window.location.origin, fetch }),
		);
		const screen = await browserRender(
			<ApiQueryProvider>
				<Detail id={1} />
			</ApiQueryProvider>,
		);
		await expect.element(screen.getByText(survey.topic)).toBeInTheDocument();
		try {
			vi.mocked(useAuth0).mockReturnValue({
				...auth,
				user: { sub: "other-user" },
			});
			await screen.rerender(
				<ApiQueryProvider>
					<Detail id={1} />
				</ApiQueryProvider>,
			);
			await expect
				.element(screen.getByText("Other user's survey"))
				.toBeInTheDocument();
			await expect
				.element(screen.getByText(survey.topic))
				.not.toBeInTheDocument();
			expect(detailRequests).toBe(2);
			expect(fetch).toHaveBeenCalledTimes(4);
		} finally {
			vi.mocked(useAuth0).mockReturnValue(auth);
		}
	});

	it("shares idempotent registration across StrictMode subscribers", async () => {
		const fetch = vi.fn(async () => response({ id: 1 }, 201));
		vi.mocked(useApiClient).mockReturnValue(
			createClient<paths>({ baseUrl: window.location.origin, fetch }),
		);
		function Registration() {
			const registration = useUserRegistration();
			return (
				<span>{registration.isSuccess ? "Registered" : "Registering"}</span>
			);
		}
		const screen = await browserRender(
			<StrictMode>
				<ApiQueryProvider>
					<Registration />
					<Registration />
				</ApiQueryProvider>
			</StrictMode>,
		);
		await expect
			.element(screen.getByText("Registered").first())
			.toBeInTheDocument();
		expect(fetch).toHaveBeenCalledTimes(1);
	});
});
