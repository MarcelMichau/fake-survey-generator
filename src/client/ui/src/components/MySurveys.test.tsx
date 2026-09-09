import { beforeEach, describe, expect, it, vi } from "vitest";
import * as hooks from "../hooks";
import { render } from "../test/test-utils";
import type { UserSurveyModel } from "../types";
import MySurveys from "./MySurveys";

vi.mock("../hooks");

type ApiCall = (url: string, options?: RequestInit) => Promise<Response>;

describe("MySurveys Component", () => {
	const mockSurveysData: UserSurveyModel[] = [
		{
			id: 1,
			topic: "What's your favorite programming language?",
			respondentType: "Developers",
			numberOfRespondents: 50,
			numberOfOptions: 3,
			winningOption: "TypeScript",
			winningOptionNumberOfVotes: 20,
		},
		{
			id: 2,
			topic: "What's your favorite color?",
			respondentType: "Color Enthusiasts",
			numberOfRespondents: 100,
			numberOfOptions: 3,
			winningOption: "Red",
			winningOptionNumberOfVotes: 50,
		},
	];

	beforeEach(() => {
		vi.clearAllMocks();
		vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: vi.fn() });
	});

	describe("Rendering", () => {
		it("should render My Surveys heading", async () => {
			const screen = await render(<MySurveys loading={false} />);
			await expect.element(screen.getByText("My Surveys")).toBeInTheDocument();
		});

		it("should have Get My Surveys button", async () => {
			const screen = await render(<MySurveys loading={false} />);
			await expect
				.element(screen.getByRole("button", { name: /Get My Surveys/i }))
				.toBeInTheDocument();
		});

		it("should show loading skeleton when loading prop is true", async () => {
			const { container } = await render(<MySurveys loading />);
			expect(
				container.querySelectorAll(".react-loading-skeleton").length,
			).toBeGreaterThan(0);
		});
	});

	describe("Initial State", () => {
		it("should not display surveys initially", async () => {
			const screen = await render(<MySurveys loading={false} />);

			await expect
				.element(screen.getByRole("button", { name: /Get My Surveys/i }))
				.toBeInTheDocument();
			await expect.element(screen.getByRole("table")).not.toBeInTheDocument();
		});

		it("should not show the empty message before fetching", async () => {
			const screen = await render(<MySurveys loading={false} />);
			await expect
				.element(screen.getByText("You have not created any surveys yet. :("))
				.not.toBeInTheDocument();
		});
	});

	describe("Fetching Surveys", () => {
		it("should call apiCall when Get My Surveys button is clicked", async () => {
			const mockApiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => mockSurveysData,
			});
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);

			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			await expect.poll(() => mockApiCall).toHaveBeenCalled();
		});

		it("should display surveys in table when data is fetched", async () => {
			const mockApiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => mockSurveysData,
			});
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);

			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			await expect
				.element(screen.getByText("What's your favorite programming language?"))
				.toBeInTheDocument();
			await expect
				.element(screen.getByText("What's your favorite color?"))
				.toBeInTheDocument();
		});

		it("should display survey table headers", async () => {
			const mockApiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => mockSurveysData,
			});
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);

			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			await expect.element(screen.getByText("Question")).toBeInTheDocument();
			await expect
				.element(screen.getByText("Winning # Votes"))
				.toBeInTheDocument();
		});
	});

	describe("Survey Display", () => {
		async function loadSurveys() {
			const mockApiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => mockSurveysData,
			});
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);
			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			return screen;
		}

		it("should display survey data in correct columns", async () => {
			const screen = await loadSurveys();
			for (const text of [
				"Developers",
				"Color Enthusiasts",
				"TypeScript",
				"Red",
			]) {
				await expect.element(screen.getByText(text)).toBeInTheDocument();
			}
		});

		it("should display survey count information", async () => {
			const screen = await loadSurveys();
			await expect.element(screen.getByText("3").first()).toBeInTheDocument();
		});
	});

	describe("Error Handling", () => {
		it("should display error message when fetch fails", async () => {
			const mockApiCall = vi.fn().mockResolvedValue({
				ok: false,
				status: 400,
				statusText: "Bad Request",
			});
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);

			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			await expect
				.element(screen.getByText("Failed to fetch surveys"))
				.toBeInTheDocument();
		});

		it("should display error message on network failure", async () => {
			const mockApiCall = vi.fn().mockRejectedValue(new Error("Network error"));
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);

			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			await expect
				.element(screen.getByText("Network error"))
				.toBeInTheDocument();
		});
	});

	describe("Empty State", () => {
		it("should display empty state when no surveys exist", async () => {
			const mockApiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => [],
			});
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);

			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			await expect
				.element(screen.getByText("You have not created any surveys yet. :("))
				.toBeInTheDocument();
		});
	});

	describe("User Interactions", () => {
		it("should allow the survey table to be displayed", async () => {
			const mockApiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => mockSurveysData,
			});
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);

			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			await expect.element(screen.getByRole("table")).toBeInTheDocument();
		});

		it("should fetch surveys when button is clicked multiple times", async () => {
			const mockApiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => mockSurveysData,
			});
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);
			const button = screen.getByRole("button", { name: /Get My Surveys/i });

			await button.click();
			await expect.poll(() => mockApiCall).toHaveBeenCalledTimes(1);
			await button.click();
			await expect.poll(() => mockApiCall).toHaveBeenCalledTimes(2);
		});
	});

	describe("Loading State", () => {
		it("should show loading state while fetching", async () => {
			const mockApiCall = vi
				.fn()
				.mockImplementation(
					() =>
						new Promise((resolve) =>
							setTimeout(
								() => resolve({ ok: true, json: async () => mockSurveysData }),
								100,
							),
						),
				);
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);

			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			await expect
				.element(screen.getByText("Searching..."))
				.toBeInTheDocument();
			await expect.poll(() => mockApiCall).toHaveBeenCalled();
		});
	});

	describe("Integration", () => {
		it("should update surveys list when fetch completes", async () => {
			const mockApiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => [mockSurveysData[0]],
			});
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);

			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			await expect
				.element(screen.getByText("What's your favorite programming language?"))
				.toBeInTheDocument();
			await expect
				.element(screen.getByText("What's your favorite color?"))
				.not.toBeInTheDocument();
		});

		it("should handle rapid button clicks", async () => {
			const mockApiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => mockSurveysData,
			});
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			const screen = await render(<MySurveys loading={false} />);
			const button = screen.getByRole("button", { name: /Get My Surveys/i });

			await button.click();
			await button.click();
			await button.click();
			await expect
				.element(screen.getByText("What's your favorite programming language?"))
				.toBeInTheDocument();
		});
	});

	describe("Delete Survey", () => {
		const fetchAndDeleteSetup = async (apiCall: ReturnType<typeof vi.fn>) => {
			vi.mocked(hooks.useApiCall).mockReturnValue({
				apiCall: apiCall as ApiCall,
			});
			const screen = await render(<MySurveys loading={false} />);
			await screen.getByRole("button", { name: /Get My Surveys/i }).click();
			await expect
				.element(screen.getByText("What's your favorite programming language?"))
				.toBeInTheDocument();
			return screen;
		};

		it("should render a delete button for each survey row", async () => {
			const apiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => mockSurveysData,
			});
			const screen = await fetchAndDeleteSetup(apiCall);
			expect(
				screen.getByRole("button", { name: /Delete survey /i }).all(),
			).toHaveLength(2);
		});

		it("should open the confirm dialog when a row's delete button is clicked", async () => {
			const apiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => mockSurveysData,
			});
			const screen = await fetchAndDeleteSetup(apiCall);

			await screen
				.getByRole("button", {
					name: /Delete survey What's your favorite programming language\?/i,
				})
				.click();
			await expect
				.element(screen.getByRole("dialog", { name: /Delete survey\?/i }))
				.toBeInTheDocument();
		});

		it("should send DELETE request and remove the row on success", async () => {
			const apiCall = vi.fn().mockResolvedValueOnce({
				ok: true,
				json: async () => mockSurveysData,
			});
			const screen = await fetchAndDeleteSetup(apiCall);
			apiCall.mockResolvedValueOnce({ ok: true, status: 204 });

			await screen
				.getByRole("button", {
					name: /Delete survey What's your favorite programming language\?/i,
				})
				.click();
			await screen.getByRole("button", { name: /^Delete$/ }).click();

			await expect
				.poll(() => apiCall)
				.toHaveBeenLastCalledWith("api/survey/1", {
					method: "DELETE",
				});
			await expect
				.element(screen.getByText("What's your favorite programming language?"))
				.not.toBeInTheDocument();
			await expect
				.element(screen.getByText("What's your favorite color?"))
				.toBeInTheDocument();
		});

		it("should surface an error alert if DELETE fails", async () => {
			const apiCall = vi.fn().mockResolvedValueOnce({
				ok: true,
				json: async () => mockSurveysData,
			});
			const screen = await fetchAndDeleteSetup(apiCall);
			apiCall.mockResolvedValueOnce({ ok: false, status: 403 });

			await screen
				.getByRole("button", {
					name: /Delete survey What's your favorite programming language\?/i,
				})
				.click();
			await screen.getByRole("button", { name: /^Delete$/ }).click();

			await expect
				.element(screen.getByText(/Failed to delete survey/i))
				.toBeInTheDocument();
			await expect
				.element(screen.getByText("What's your favorite programming language?"))
				.toBeInTheDocument();
		});

		it("should close the dialog when Cancel is clicked", async () => {
			const apiCall = vi.fn().mockResolvedValue({
				ok: true,
				json: async () => mockSurveysData,
			});
			const screen = await fetchAndDeleteSetup(apiCall);

			await screen
				.getByRole("button", {
					name: /Delete survey What's your favorite programming language\?/i,
				})
				.click();
			await expect
				.element(screen.getByRole("dialog", { name: /Delete survey\?/i }))
				.toBeInTheDocument();

			await screen.getByRole("button", { name: /Cancel/i }).click();
			await expect
				.element(screen.getByRole("dialog", { name: /Delete survey\?/i }))
				.not.toBeInTheDocument();
		});
	});
});
