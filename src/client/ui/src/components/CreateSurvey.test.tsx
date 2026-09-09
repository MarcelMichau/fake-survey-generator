import { beforeEach, describe, expect, it, vi } from "vitest";
import * as hooks from "../hooks";
import { render } from "../test/test-utils";
import CreateSurvey from "./CreateSurvey";

vi.mock("../hooks");

describe("CreateSurvey Component", () => {
	const mockApiCall = vi.fn();
	const mockOnSurveyCreated = vi.fn();

	beforeEach(() => {
		vi.clearAllMocks();
		mockApiCall.mockResolvedValue({
			ok: true,
			status: 201,
			json: async () => ({ id: 123, topic: "Test Survey" }),
		});
		vi.mocked(hooks.useApiCall).mockReturnValue({ apiCall: mockApiCall });
	});

	describe("Rendering", () => {
		it("should render create survey form", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);

			await expect
				.element(screen.getByRole("heading", { name: /Create Survey/i }))
				.toBeInTheDocument();
			await expect
				.element(screen.getByPlaceholder("Pragmatic Developers"))
				.toBeInTheDocument();
			await expect
				.element(screen.getByPlaceholder("Do you prefer tabs or spaces?"))
				.toBeInTheDocument();
		});

		it("should show loading skeleton when loading prop is true", async () => {
			const { container } = await render(
				<CreateSurvey loading onSurveyCreated={mockOnSurveyCreated} />,
			);

			const skeletons = container.querySelectorAll(".react-loading-skeleton");
			expect(skeletons.length).toBeGreaterThan(0);
		});

		it("should have submit button", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);

			await expect
				.element(screen.getByRole("button", { name: /Create Survey/i }))
				.toBeInTheDocument();
		});
	});

	describe("Form Interactions", () => {
		it("should update form fields when user fills them", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);

			const respondentInput = screen.getByPlaceholder("Pragmatic Developers");
			const questionInput = screen.getByPlaceholder(
				"Do you prefer tabs or spaces?",
			);

			await respondentInput.fill("Tech Developers");
			await questionInput.fill("What's your favorite color?");

			await expect.element(respondentInput).toHaveValue("Tech Developers");
			await expect
				.element(questionInput)
				.toHaveValue("What's your favorite color?");
		});

		it("should add new survey option when Add Option button is clicked", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			const addButton = screen.getByRole("button", { name: /Add Option/i });

			await addButton.click();
			expect(screen.getByPlaceholder("Some other option").all()).toHaveLength(
				1,
			);

			await addButton.click();
			expect(screen.getByPlaceholder("Some other option").all()).toHaveLength(
				2,
			);
		});

		it("should remove option when Remove button is clicked", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			const addButton = screen.getByRole("button", { name: /Add Option/i });

			await addButton.click();
			await screen.getByRole("button", { name: /Remove #2/i }).click();

			await expect
				.element(screen.getByRole("button", { name: /Remove #2/i }))
				.not.toBeInTheDocument();
		});

		it("should generate unique option IDs even after removing options", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			const addButton = screen.getByRole("button", { name: /Add Option/i });

			await addButton.click();
			await expect
				.element(screen.getByRole("button", { name: /Remove #2/i }))
				.toBeInTheDocument();
			await addButton.click();
			await expect
				.element(screen.getByRole("button", { name: /Remove #3/i }))
				.toBeInTheDocument();

			await screen.getByRole("button", { name: /Remove #2/i }).click();
			await expect
				.element(screen.getByRole("button", { name: /Remove #2/i }))
				.not.toBeInTheDocument();

			await addButton.click();
			await expect
				.element(screen.getByRole("button", { name: /Remove #4/i }))
				.toBeInTheDocument();
			await expect
				.element(screen.getByRole("button", { name: /Remove #2/i }))
				.not.toBeInTheDocument();
		});

		it("should update option text when user fills the option field", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			const optionInput = screen.getByPlaceholder("Most definitely tabs");

			await optionInput.fill("Red");
			await expect.element(optionInput).toHaveValue("Red");
		});

		it("should render the preferred votes number input", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			const preferredVotesInput = screen.getByRole("spinbutton").first();

			await expect
				.element(preferredVotesInput)
				.toHaveAttribute("type", "number");
		});
	});

	describe("Form Submission", () => {
		it("should submit form with survey data", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			mockApiCall.mockResolvedValue({
				ok: true,
				status: 201,
				json: async () => ({ id: 456, topic: "Color Preference" }),
			});

			await screen
				.getByPlaceholder("Pragmatic Developers")
				.fill("Test Audience");
			await screen
				.getByPlaceholder("Do you prefer tabs or spaces?")
				.fill("Test Question");
			await screen.getByPlaceholder("Most definitely tabs").fill("Option 1");
			await screen.getByRole("button", { name: /Create Survey/i }).click();

			await expect
				.poll(() => mockApiCall)
				.toHaveBeenCalledWith("api/survey", {
					method: "POST",
					body: expect.stringContaining("Test Audience"),
				});
		});

		it("should call onSurveyCreated callback on successful submission", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			mockApiCall.mockResolvedValue({
				ok: true,
				status: 201,
				json: async () => ({ id: 789, topic: "Test" }),
			});

			await screen.getByRole("button", { name: /Create Survey/i }).click();

			await expect.poll(() => mockOnSurveyCreated).toHaveBeenCalledWith(789);
		});

		it("should show success message on successful submission", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			mockApiCall.mockResolvedValue({
				ok: true,
				status: 201,
				json: async () => ({ id: 999, topic: "Test Survey" }),
			});

			await screen.getByRole("button", { name: /Create Survey/i }).click();

			await expect
				.element(screen.getByText(/Survey created with ID: 999/))
				.toBeInTheDocument();
		});

		it("should show error message on failed submission", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			mockApiCall.mockResolvedValue({ ok: false, status: 500 });

			await screen.getByRole("button", { name: /Create Survey/i }).click();

			await expect
				.element(
					screen.getByText(/Please try again or create an issue on GitHub/),
				)
				.toBeInTheDocument();
		});

		it("should show validation errors on 422 response", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			mockApiCall.mockResolvedValue({
				ok: false,
				status: 422,
				json: async () => ({
					surveyTopic: ["Topic is required"],
					numberOfRespondents: ["Must be greater than 0"],
				}),
			});

			await screen.getByRole("button", { name: /Create Survey/i }).click();

			await expect
				.element(screen.getByText("Topic is required"))
				.toBeInTheDocument();
			await expect
				.element(screen.getByText("Must be greater than 0"))
				.toBeInTheDocument();
		});

		it("should show duplicate option error on 422 domain exception response", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			mockApiCall.mockResolvedValue({
				ok: false,
				status: 422,
				json: async () => ({
					"survey.domain.exception": ["Duplicate survey option."],
				}),
			});

			await screen.getByRole("button", { name: /Create Survey/i }).click();

			await expect
				.element(screen.getByText("Duplicate survey option."))
				.toBeInTheDocument();
		});

		it("should reset form after successful submission when resetOnSuccess is true", async () => {
			const screen = await render(
				<CreateSurvey loading={false} resetOnSuccess />,
			);
			mockApiCall.mockResolvedValue({
				ok: true,
				status: 201,
				json: async () => ({ id: 111, topic: "Test" }),
			});
			const respondentInput = screen.getByPlaceholder("Pragmatic Developers");
			const questionInput = screen.getByPlaceholder(
				"Do you prefer tabs or spaces?",
			);

			await respondentInput.fill("Test");
			await questionInput.fill("Test");
			await screen.getByRole("button", { name: /Create Survey/i }).click();

			await expect.element(respondentInput).toHaveValue("");
			await expect.element(questionInput).toHaveValue("");
		});

		it("should not reset form after successful submission when onSurveyCreated is provided", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			mockApiCall.mockResolvedValue({
				ok: true,
				status: 201,
				json: async () => ({ id: 111, topic: "Test" }),
			});
			const respondentInput = screen.getByPlaceholder("Pragmatic Developers");
			const questionInput = screen.getByPlaceholder(
				"Do you prefer tabs or spaces?",
			);

			await respondentInput.fill("Test");
			await questionInput.fill("Test");
			await screen.getByRole("button", { name: /Create Survey/i }).click();

			await expect.poll(() => mockOnSurveyCreated).toHaveBeenCalledWith(111);
			await expect.element(respondentInput).toHaveValue("Test");
			await expect.element(questionInput).toHaveValue("Test");
		});

		it("should reset form after successful submission when resetOnSuccess is explicitly true with onSurveyCreated", async () => {
			const screen = await render(
				<CreateSurvey
					loading={false}
					onSurveyCreated={mockOnSurveyCreated}
					resetOnSuccess
				/>,
			);
			mockApiCall.mockResolvedValue({
				ok: true,
				status: 201,
				json: async () => ({ id: 111, topic: "Test" }),
			});
			const respondentInput = screen.getByPlaceholder("Pragmatic Developers");
			const questionInput = screen.getByPlaceholder(
				"Do you prefer tabs or spaces?",
			);

			await respondentInput.fill("Test");
			await questionInput.fill("Test");
			await screen.getByRole("button", { name: /Create Survey/i }).click();

			await expect.poll(() => mockOnSurveyCreated).toHaveBeenCalledWith(111);
			await expect.element(respondentInput).toHaveValue("");
			await expect.element(questionInput).toHaveValue("");
		});
	});

	describe("State Management", () => {
		it("should properly initialize form state", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);

			await expect
				.element(screen.getByPlaceholder("Pragmatic Developers"))
				.toHaveValue("");
			await expect
				.element(screen.getByPlaceholder("Do you prefer tabs or spaces?"))
				.toHaveValue("");
		});
	});
});
