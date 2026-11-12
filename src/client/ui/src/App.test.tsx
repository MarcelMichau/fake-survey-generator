import { useAuth0 } from "@auth0/auth0-react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { userEvent } from "vitest/browser";
import { render } from "vitest-browser-react";
import "./index.css";
import App from "./App";
import { ApiQueryProvider } from "./api/ApiQueryProvider";
import { useApiClient } from "./hooks/useApiClient";
import { mockApiClient } from "./test/mock-api-client";

vi.mock("./hooks/useApiClient");
vi.mock("./components/NavBar", () => ({ default: () => null }));
vi.mock("./components/Footer", () => ({ default: () => null }));

// Readiness presentation belongs to App; real query lifecycle is covered separately.
vi.mock("./components/CreateSurvey", () => ({
	default: ({ loading }: { loading: boolean }) => (
		<button type="button" disabled={loading}>
			Create Survey
		</button>
	),
}));
vi.mock("./components/GetSurvey", () => ({
	default: ({ loading }: { loading: boolean }) => (
		<button type="button" disabled={loading}>
			Get Survey
		</button>
	),
}));
vi.mock("./components/MySurveys", () => ({
	default: ({ loading }: { loading: boolean }) => (
		<button type="button" disabled={loading}>
			Get My Surveys
		</button>
	),
}));

describe("App registration presentation", () => {
	beforeEach(() => vi.clearAllMocks());

	it.each([false, true])(
		"provides one main landmark and a working skip link when authenticated=%s",
		async (authenticated) => {
			const previous = useAuth0();
			vi.mocked(useAuth0).mockReturnValue({
				...previous,
				isAuthenticated: authenticated,
			});
			vi.mocked(useApiClient).mockReturnValue(
				mockApiClient(
					vi.fn().mockResolvedValue({
						ok: true,
						status: 201,
						json: async () => ({ id: 1 }),
					}),
				),
			);
			try {
				const screen = await render(
					<ApiQueryProvider>
						<App />
					</ApiQueryProvider>,
				);
				expect(screen.getByRole("main").all()).toHaveLength(1);
				expect(screen.getByRole("heading", { level: 1 }).all()).toHaveLength(1);
				screen
					.getByRole("link", { name: "Skip to main content" })
					.element()
					.focus();
				await userEvent.keyboard("{Enter}");
				await expect
					.element(screen.getByRole("heading", { level: 1 }))
					.toHaveFocus();
			} finally {
				vi.mocked(useAuth0).mockReturnValue(previous);
			}
		},
	);

	it("keeps controls unavailable after failure and enables them after explicit registration retry", async () => {
		const transport = vi
			.fn()
			.mockResolvedValueOnce({ ok: false, status: 503 })
			.mockResolvedValue({
				ok: true,
				status: 201,
				json: async () => ({ id: 1 }),
			});
		vi.mocked(useApiClient).mockReturnValue(mockApiClient(transport));
		const screen = await render(
			<ApiQueryProvider>
				<App />
			</ApiQueryProvider>,
		);
		await expect
			.element(screen.getByRole("button", { name: "Retry registration" }))
			.toBeInTheDocument();
		for (const name of ["Create Survey", "Get Survey", "Get My Surveys"]) {
			await expect
				.element(screen.getByRole("button", { name, exact: true }))
				.toBeDisabled();
		}
		expect(transport).toHaveBeenCalledTimes(1);
		await screen.getByRole("button", { name: "Retry registration" }).click();
		for (const name of ["Create Survey", "Get Survey", "Get My Surveys"]) {
			await expect
				.element(screen.getByRole("button", { name, exact: true }))
				.not.toBeDisabled();
		}
		await expect
			.element(screen.getByRole("button", { name: "Retry registration" }))
			.not.toBeInTheDocument();
		expect(transport).toHaveBeenCalledTimes(2);
	});
});
