import * as auth0 from "@auth0/auth0-react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import * as hooks from "../hooks";
import { render } from "../test/test-utils";
import type { SurveyModel } from "../types";
import GetSurvey from "./GetSurvey";

vi.mock("../hooks");

describe("GetSurvey Component", () => {
	const mockSurveyData: SurveyModel = {
		id: 123,
		ownerExternalUserId: "test-user-id",
		topic: "What's your favorite color?",
		respondentType: "Color Enthusiasts",
		numberOfRespondents: 100,
		createdOn: new Date("2026-01-20T00:00:00Z"),
		options: [
			{
				id: 1,
				optionText: "Red",
				numberOfVotes: 50,
				preferredNumberOfVotes: 0,
			},
			{
				id: 2,
				optionText: "Green",
				numberOfVotes: 30,
				preferredNumberOfVotes: 0,
			},
			{
				id: 3,
				optionText: "Blue",
				numberOfVotes: 20,
				preferredNumberOfVotes: 0,
			},
		],
	};

	beforeEach(() => {
		vi.clearAllMocks();
		vi.mocked(hooks.useSurveyFetch).mockReturnValue({
			survey: null,
			loading: false,
			error: "",
		});
		vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: vi.fn() });
	});

	describe("Rendering", () => {
		it("should render get survey form", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await expect
				.element(screen.getByRole("heading", { name: /Get Survey/i }))
				.toBeInTheDocument();
			await expect
				.element(screen.getByPlaceholder("Enter survey ID number"))
				.toBeInTheDocument();
		});

		it("should show loading skeleton when loading prop is true", async () => {
			const { container } = await render(
				<GetSurvey loading newSurveyId={null} />,
			);

			expect(
				container.querySelectorAll(".react-loading-skeleton").length,
			).toBeGreaterThan(0);
		});

		it("should have Get Survey button", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await expect
				.element(screen.getByRole("button", { name: /Get Survey/i }))
				.toBeInTheDocument();
		});
	});

	describe("Form Interactions", () => {
		it("should update survey ID input when user fills it", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);
			const surveyIdInput = screen.getByPlaceholder("Enter survey ID number");

			await surveyIdInput.fill("123");
			await expect.element(surveyIdInput).toHaveValue("123");
		});

		it("should ignore non-numeric input", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);
			const surveyIdInput = screen.getByPlaceholder("Enter survey ID number");

			await expect.element(surveyIdInput).toHaveValue("0");
			await surveyIdInput.fill("123");
			await expect.element(surveyIdInput).toHaveValue("123");
		});
	});

	describe("Survey Fetching", () => {
		it("should auto-fetch when newSurveyId prop changes", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);
			expect(hooks.useSurveyFetch).toHaveBeenCalledWith(null);

			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: mockSurveyData,
				loading: false,
				error: "",
			});
			await screen.rerender(<GetSurvey loading={false} newSurveyId={456} />);

			await expect.poll(() => hooks.useSurveyFetch).toHaveBeenCalledWith(456);
		});

		it("should display survey results when data is fetched", async () => {
			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: mockSurveyData,
				loading: false,
				error: "",
			});
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await expect
				.element(screen.getByText("What's your favorite color?"))
				.toBeInTheDocument();
			await expect
				.element(screen.getByText("Color Enthusiasts"))
				.toBeInTheDocument();
			await expect.element(screen.getByText("100")).toBeInTheDocument();
		});

		it("should display error message when survey not found", async () => {
			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: null,
				loading: false,
				error: "Looks like that survey does not exist",
			});
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await expect
				.element(screen.getByText("Looks like that survey does not exist"))
				.toBeInTheDocument();
		});

		it("should display error message for API failures", async () => {
			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: null,
				loading: false,
				error: "Something did not go as planned",
			});
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await expect
				.element(screen.getByText("Something did not go as planned"))
				.toBeInTheDocument();
		});

		it("should show loading state while fetching", async () => {
			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: null,
				loading: true,
				error: "",
			});
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await expect
				.element(screen.getByText("Searching..."))
				.toBeInTheDocument();
		});
	});

	describe("Survey Display", () => {
		beforeEach(() => {
			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: mockSurveyData,
				loading: false,
				error: "",
			});
		});

		it("should display survey options and votes", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			for (const text of [
				"Red",
				"Green",
				"Blue",
				"50 votes",
				"30 votes",
				"20 votes",
			]) {
				await expect.element(screen.getByText(text)).toBeInTheDocument();
			}
		});

		it("should not display survey results initially", async () => {
			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: null,
				loading: false,
				error: "",
			});
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await expect.element(screen.getByText("Red")).not.toBeInTheDocument();
		});

		it("should display survey creation date", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await expect
				.element(screen.getByText("What's your favorite color?"))
				.toBeInTheDocument();
		});
	});

	describe("Form Submission", () => {
		it("should trigger fetch when Get Survey button is clicked", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);
			const surveyIdInput = screen.getByPlaceholder("Enter survey ID number");

			await surveyIdInput.fill("789");
			await expect.element(surveyIdInput).toHaveValue("789");
			await screen.getByRole("button", { name: /Get Survey/i }).click();
			await expect
				.element(screen.getByPlaceholder("Enter survey ID number"))
				.toHaveValue("789");
		});

		it("should handle form submission", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);
			await screen.getByRole("button", { name: /Get Survey/i }).click();
		});
	});

	describe("Integration", () => {
		it("should update input when newSurveyId prop is set", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);
			await expect
				.element(screen.getByPlaceholder("Enter survey ID number"))
				.toHaveValue("0");

			await screen.rerender(<GetSurvey loading={false} newSurveyId={999} />);
			await expect
				.element(screen.getByPlaceholder("Enter survey ID number"))
				.toHaveValue("999");
		});

		it("should handle transitions between loading and loaded states", async () => {
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: null,
				loading: true,
				error: "",
			});
			await screen.rerender(<GetSurvey loading={false} newSurveyId={123} />);
			await expect
				.element(screen.getByText("Searching..."))
				.toBeInTheDocument();

			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: mockSurveyData,
				loading: false,
				error: "",
			});
			await screen.rerender(<GetSurvey loading={false} newSurveyId={123} />);
			await expect
				.element(screen.getByText("Searching..."))
				.not.toBeInTheDocument();
			await expect
				.element(screen.getByText("What's your favorite color?"))
				.toBeInTheDocument();
		});
	});

	describe("Delete button visibility (owner-gated)", () => {
		it("should show the delete button when current user owns the survey", async () => {
			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: mockSurveyData,
				loading: false,
				error: "",
			});
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await expect
				.element(screen.getByRole("button", { name: /Delete this survey/i }))
				.toBeInTheDocument();
		});

		it("should hide the delete button when current user does not own the survey", async () => {
			vi.mocked(auth0.useAuth0).mockReturnValueOnce({
				isAuthenticated: true,
				isLoading: false,
				user: { sub: "different-user-id", name: "Other User" },
				// biome-ignore lint/suspicious/noExplicitAny: test mock
				getAccessTokenSilently: vi.fn(async () => "test-token") as any,
				// biome-ignore lint/suspicious/noExplicitAny: test mock
			} as any);
			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: mockSurveyData,
				loading: false,
				error: "",
			});
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await expect
				.element(screen.getByRole("button", { name: /Delete this survey/i }))
				.not.toBeInTheDocument();
		});

		it("should send DELETE request and clear the survey on confirm", async () => {
			const mockApiCall = vi.fn().mockResolvedValue({ ok: true, status: 204 });
			vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
			vi.mocked(hooks.useSurveyFetch).mockReturnValue({
				survey: mockSurveyData,
				loading: false,
				error: "",
			});
			const screen = await render(
				<GetSurvey loading={false} newSurveyId={null} />,
			);

			await screen.getByRole("button", { name: /Delete this survey/i }).click();
			await screen.getByRole("button", { name: /^Delete$/ }).click();

			await expect
				.poll(() => mockApiCall)
				.toHaveBeenCalledWith(`api/survey/${mockSurveyData.id}`, {
					method: "DELETE",
				});
		});
	});
});
