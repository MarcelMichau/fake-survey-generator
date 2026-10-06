import { useAuth0 } from "@auth0/auth0-react";
import createClient from "openapi-fetch";
import { act, StrictMode, useState } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { render } from "vitest-browser-react";
import { useApiClient } from "../hooks/useApiClient";
import { useSurveyFetch } from "../hooks/useSurveyFetch";
import {
	useAnalyzeSurvey,
	useCreateSurvey,
	useDeleteSurvey,
	useUserSurveys,
} from "../hooks/useSurveys";
import { useUserRegistration } from "../hooks/useUserRegistration";
import { mockApiClient } from "../test/mock-api-client";
import { ApiQueryProvider } from "./ApiQueryProvider";
import type { paths } from "./generated";

vi.mock("../hooks/useApiClient");

type TransportResponse = {
	ok: boolean;
	status: number;
	json: () => Promise<unknown>;
};

function deferred() {
	let resolve: (response: TransportResponse) => void = () => {
		throw new Error("Deferred response not initialized");
	};
	const promise = new Promise<TransportResponse>((complete) => {
		resolve = complete;
	});
	return { promise, resolve };
}

const registered: TransportResponse = {
	ok: true,
	status: 201,
	json: async () => ({ id: 1 }),
};
const survey = {
	id: 1,
	topic: "Tabs or spaces?",
	respondentType: "Developers",
	numberOfRespondents: 100,
	options: [],
};
const created: TransportResponse = {
	ok: true,
	status: 201,
	json: async () => survey,
};
const command = {
	surveyTopic: survey.topic,
	respondentType: survey.respondentType,
	numberOfRespondents: 100,
	surveyOptions: [{ optionText: "Tabs" }],
};

function useWorkflow(readsRequested = true) {
	const registration = useUserRegistration();
	const detail = useSurveyFetch(readsRequested ? 1 : null);
	const list = useUserSurveys(readsRequested);
	const creation = useCreateSurvey();
	const analysis = useAnalyzeSurvey();
	const deletion = useDeleteSurvey();
	return { registration, detail, list, creation, analysis, deletion };
}

type Workflow = ReturnType<typeof useWorkflow>;

function ReadPresentation({ workflow }: { workflow: Workflow }) {
	return (
		<>
			<span>
				{workflow.registration.isSuccess ? "Registered" : "Not ready"}
			</span>
			<span>{workflow.detail.survey?.topic ?? "No detail"}</span>
			<span>List count: {workflow.list.data?.length ?? "pending"}</span>
		</>
	);
}

async function mountWorkflow(readsRequested = true) {
	let workflow: Workflow;
	function Probe() {
		workflow = useWorkflow(readsRequested);
		return <ReadPresentation workflow={workflow} />;
	}
	const screen = await render(
		<StrictMode>
			<ApiQueryProvider>
				<Probe />
			</ApiQueryProvider>
		</StrictMode>,
	);
	return {
		screen,
		get workflow() {
			return workflow;
		},
		element: (
			<StrictMode>
				<ApiQueryProvider>
					<Probe />
				</ApiQueryProvider>
			</StrictMode>
		),
	};
}

describe("Authenticated Survey readiness", () => {
	const transport =
		vi.fn<(url: string, options?: RequestInit) => Promise<TransportResponse>>();

	beforeEach(() => {
		vi.clearAllMocks();
		transport.mockReset();
		transport.mockImplementation(async (url, options) => {
			if (url === "api/user/register") return registered;
			if (url === "api/survey/user")
				return { ok: true, status: 200, json: async () => [survey] };
			if (options?.method === "DELETE")
				return { ok: true, status: 204, json: async () => null };
			if (url === "api/survey/analyze")
				return { ok: true, status: 200, json: async () => ({ warnings: [] }) };
			return options?.method === "POST"
				? created
				: { ok: true, status: 200, json: async () => survey };
		});
		vi.mocked(useApiClient).mockReturnValue(mockApiClient(transport));
	});

	it("shares pending registration and resumes requested reads once, including StrictMode", async () => {
		const registration = deferred();
		transport.mockImplementationOnce(() => registration.promise);
		let second: Workflow;
		function Probe() {
			const first = useWorkflow();
			second = useWorkflow();
			return <ReadPresentation workflow={first} />;
		}
		const screen = await render(
			<StrictMode>
				<ApiQueryProvider>
					<Probe />
				</ApiQueryProvider>
			</StrictMode>,
		);
		await expect.poll(() => transport.mock.calls.length).toBe(1);
		expect(transport.mock.calls[0][0]).toBe("api/user/register");
		await act(async () => {
			await second.detail.refetch();
			await second.list.refetch();
		});
		expect(transport).toHaveBeenCalledTimes(1);
		await act(async () => registration.resolve(registered));
		await expect.element(screen.getByText(survey.topic)).toBeInTheDocument();
		await expect.element(screen.getByText("List count: 1")).toBeInTheDocument();
		expect(transport.mock.calls.map(([url]) => url).sort()).toEqual(
			["api/survey/1", "api/survey/user", "api/user/register"].sort(),
		);
	});

	it("does not start unrequested reads; a manual pending list refresh retains read intent", async () => {
		const registration = deferred();
		transport.mockImplementationOnce(() => registration.promise);
		const probe = await mountWorkflow(false);
		await expect.poll(() => transport.mock.calls.length).toBe(1);
		await act(async () => probe.workflow.list.refetch());
		expect(transport).toHaveBeenCalledTimes(1);
		await act(async () => registration.resolve(registered));
		await expect
			.element(probe.screen.getByText("List count: 1"))
			.toBeInTheDocument();
		expect(transport.mock.calls.map(([url]) => url)).toEqual([
			"api/user/register",
			"api/survey/user",
		]);
	});

	it("keeps unrequested reads idle after successful registration", async () => {
		const probe = await mountWorkflow(false);
		await expect
			.element(probe.screen.getByText("Registered"))
			.toBeInTheDocument();
		expect(transport.mock.calls.map(([url]) => url)).toEqual([
			"api/user/register",
		]);
	});

	it.each(["creation", "analysis", "deletion"] as const)(
		"rejects early %s without sending or queuing it",
		async (operation) => {
			const registration = deferred();
			transport.mockImplementationOnce(() => registration.promise);
			const probe = await mountWorkflow(false);
			await expect.poll(() => transport.mock.calls.length).toBe(1);
			await act(async () => {
				const mutation =
					operation === "deletion"
						? probe.workflow.deletion.mutateAsync(1)
						: operation === "analysis"
							? probe.workflow.analysis.mutateAsync(command)
							: probe.workflow.creation.mutateAsync(command);
				await expect(mutation).rejects.toThrow(
					"until user registration succeeds",
				);
			});
			expect(transport).toHaveBeenCalledTimes(1);
			await act(async () => registration.resolve(registered));
			await expect
				.element(probe.screen.getByText("Registered"))
				.toBeInTheDocument();
			expect(transport).toHaveBeenCalledTimes(1);
			await act(async () => {
				if (operation === "deletion")
					await probe.workflow.deletion.mutateAsync(1);
				else if (operation === "analysis")
					await probe.workflow.analysis.mutateAsync(command);
				else await probe.workflow.creation.mutateAsync(command);
			});
			expect(transport).toHaveBeenCalledTimes(2);
		},
	);

	it("keeps failed registration blocked across new callers and Survey refreshes until explicit retry", async () => {
		transport.mockResolvedValueOnce({
			ok: false,
			status: 503,
			json: async () => ({}),
		});
		let first: Workflow;
		let addCaller: () => void;
		function ExtraCaller() {
			useWorkflow();
			return null;
		}
		function Probe() {
			first = useWorkflow();
			const [extra, setExtra] = useState(false);
			addCaller = () => setExtra(true);
			return (
				<>
					<ReadPresentation workflow={first} />
					{extra && <ExtraCaller />}
				</>
			);
		}
		const screen = await render(
			<ApiQueryProvider>
				<Probe />
			</ApiQueryProvider>,
		);
		await expect.poll(() => first.registration.isError).toBe(true);
		await act(async () => {
			addCaller();
			await first.detail.refetch();
			await first.list.refetch();
			await expect(first.creation.mutateAsync(command)).rejects.toThrow(
				"until user registration succeeds",
			);
		});
		expect(transport).toHaveBeenCalledTimes(1);
		await act(async () => first.registration.refetch());
		await expect.element(screen.getByText(survey.topic)).toBeInTheDocument();
		await expect.element(screen.getByText("List count: 1")).toBeInTheDocument();
		expect(
			transport.mock.calls.filter(([url]) => url === "api/user/register"),
		).toHaveLength(2);
		expect(transport).toHaveBeenCalledTimes(4);
	});

	it.each(["switch", "logout"] as const)(
		"discards old pending registration and read intent on %s",
		async (transition) => {
			const auth = useAuth0();
			const old = deferred();
			const next = deferred();
			transport
				.mockImplementationOnce(() => old.promise)
				.mockImplementationOnce(() => next.promise);
			const probe = await mountWorkflow();
			await expect.poll(() => transport.mock.calls.length).toBe(1);
			try {
				vi.mocked(useAuth0).mockReturnValue({
					...auth,
					isAuthenticated: transition === "switch",
					user: transition === "switch" ? { sub: "other-user" } : undefined,
				});
				await probe.screen.rerender(probe.element);
				await expect
					.poll(() => transport.mock.calls.length)
					.toBe(transition === "switch" ? 2 : 1);
				await act(async () => old.resolve(registered));
				expect(
					transport.mock.calls.some(([url]) => url.startsWith("api/survey")),
				).toBe(false);
				if (transition === "switch") {
					await act(async () => next.resolve(registered));
					await expect
						.element(probe.screen.getByText(survey.topic))
						.toBeInTheDocument();
					expect(transport).toHaveBeenCalledTimes(4);
				} else {
					await act(async () => {
						await probe.workflow.detail.refetch();
						await expect(
							probe.workflow.analysis.mutateAsync(command),
						).rejects.toThrow("until user registration succeeds");
					});
					expect(transport).toHaveBeenCalledTimes(1);
				}
			} finally {
				vi.mocked(useAuth0).mockReturnValue(auth);
			}
		},
	);

	it("does not register or send Survey work without an authenticated identity", async () => {
		const auth = useAuth0();
		try {
			vi.mocked(useAuth0).mockReturnValue({ ...auth, user: undefined });
			const probe = await mountWorkflow();
			await act(async () => {
				await probe.workflow.registration.refetch();
				await probe.workflow.detail.refetch();
				await probe.workflow.list.refetch();
				await expect(
					probe.workflow.creation.mutateAsync(command),
				).rejects.toThrow("until user registration succeeds");
			});
			expect(transport).not.toHaveBeenCalled();
		} finally {
			vi.mocked(useAuth0).mockReturnValue(auth);
		}
	});

	it("cancels pending registration transport when the session is discarded", async () => {
		const auth = useAuth0();
		let requestSignal: AbortSignal | undefined;
		const fetch = vi.fn((request: Request) => {
			requestSignal = request.signal;
			return new Promise<Response>((_, reject) => {
				request.signal.addEventListener("abort", () =>
					reject(request.signal.reason),
				);
			});
		});
		vi.mocked(useApiClient).mockReturnValue(
			createClient<paths>({ baseUrl: window.location.origin, fetch }),
		);
		const probe = await mountWorkflow();
		await expect.poll(() => requestSignal).toBeDefined();
		try {
			vi.mocked(useAuth0).mockReturnValue({
				...auth,
				isAuthenticated: false,
				user: undefined,
			});
			await probe.screen.rerender(probe.element);
			await expect.poll(() => requestSignal?.aborted).toBe(true);
			expect(fetch).toHaveBeenCalledTimes(1);
		} finally {
			vi.mocked(useAuth0).mockReturnValue(auth);
		}
	});

	it("cancels pending mutation transport on logout", async () => {
		const auth = useAuth0();
		let mutationSignal: AbortSignal | undefined;
		const fetch = vi.fn((request: Request) => {
			if (request.url.endsWith("/api/user/register")) {
				return Promise.resolve(
					new Response(JSON.stringify({ id: 1 }), {
						status: 201,
						headers: { "Content-Type": "application/json" },
					}),
				);
			}
			mutationSignal = request.signal;
			return new Promise<Response>((_, reject) => {
				request.signal.addEventListener("abort", () =>
					reject(request.signal.reason),
				);
			});
		});
		vi.mocked(useApiClient).mockReturnValue(
			createClient<paths>({ baseUrl: window.location.origin, fetch }),
		);
		const probe = await mountWorkflow(false);
		await expect
			.element(probe.screen.getByText("Registered"))
			.toBeInTheDocument();
		let pending: Promise<unknown> = Promise.resolve();
		await act(async () => {
			pending = probe.workflow.analysis
				.mutateAsync(command)
				.catch((error) => error);
		});
		await expect.poll(() => mutationSignal).toBeDefined();
		try {
			vi.mocked(useAuth0).mockReturnValue({
				...auth,
				isAuthenticated: false,
				user: undefined,
			});
			await probe.screen.rerender(probe.element);
			await expect.poll(() => mutationSignal?.aborted).toBe(true);
			await pending;
			expect(fetch).toHaveBeenCalledTimes(2);
		} finally {
			vi.mocked(useAuth0).mockReturnValue(auth);
		}
	});

	it("discards late creation outcomes and callbacks after a User switch", async () => {
		const auth = useAuth0();
		const creation = deferred();
		transport
			.mockImplementationOnce(async () => registered)
			.mockImplementationOnce(() => creation.promise);
		const probe = await mountWorkflow(false);
		await expect
			.element(probe.screen.getByText("Registered"))
			.toBeInTheDocument();
		const success = vi.fn();
		let pending: Promise<unknown>;
		await act(async () => {
			pending = probe.workflow.creation
				.mutateAsync(command, { onSuccess: success })
				.catch((error) => error);
		});
		await expect.poll(() => transport.mock.calls.length).toBe(2);
		try {
			vi.mocked(useAuth0).mockReturnValue({
				...auth,
				user: { sub: "other-user" },
			});
			await probe.screen.rerender(probe.element);
			await expect
				.element(probe.screen.getByText("Registered"))
				.toBeInTheDocument();
			await act(async () => {
				creation.resolve(created);
				await pending;
			});
			expect(success).not.toHaveBeenCalled();
			expect(probe.workflow.detail.survey).toBe(null);
			expect(transport).toHaveBeenCalledTimes(3);
		} finally {
			vi.mocked(useAuth0).mockReturnValue(auth);
		}
	});
});
