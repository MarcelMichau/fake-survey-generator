import { beforeEach, describe, expect, it, vi } from "vitest";
import { page } from "vitest/browser";
import * as hooks from "../hooks/useApiClient";
import { mockApiClient } from "../test/mock-api-client";
import { render } from "../test/test-utils";
import CreateSurvey from "./CreateSurvey";

vi.mock("../hooks/useApiClient");

async function renderValidSurvey(ui: Parameters<typeof render>[0]) {
	const screen = await render(ui);
	await screen.getByPlaceholder("Pragmatic Developers").fill("Developers");
	await screen
		.getByPlaceholder("Do you prefer tabs or spaces?")
		.fill("Tabs or spaces?");
	await screen
		.getByRole("spinbutton", { name: "Number of Respondents" })
		.fill("100");
	await screen.getByPlaceholder("Most definitely tabs").fill("Tabs");
	return screen;
}

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
		vi.mocked(hooks.useApiClient).mockReturnValue(mockApiClient(mockApiCall));
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
			await expect
				.element(
					screen.getByText(
						"Analyse provided survey information for potential issues.",
					),
				)
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

		it("should keep option labels sequential after removing and adding options", async () => {
			const screen = await render(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			const addButton = screen.getByRole("button", { name: /Add Option/i });

			await addButton.click();
			await expect
				.element(screen.getByRole("button", { name: /Remove #2/i }))
				.toBeInTheDocument();

			await screen.getByRole("button", { name: /Remove #2/i }).click();
			await expect
				.element(screen.getByRole("button", { name: /Remove #2/i }))
				.not.toBeInTheDocument();

			await addButton.click();
			await expect
				.element(screen.getByTestId("field-label").filter({ hasText: /^#2$/ }))
				.toBeInTheDocument();
			await expect
				.element(screen.getByRole("button", { name: /Remove #2/i }))
				.toBeInTheDocument();
			await expect
				.element(screen.getByTestId("field-label").filter({ hasText: /^#3$/ }))
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

	describe("Survey Analysis", () => {
		it("should disable form inputs while analysis is in progress", async () => {
			let completeAnalysis: (() => void) | undefined;
			mockApiCall.mockImplementation(
				() =>
					new Promise<Response>((resolve) => {
						completeAnalysis = () =>
							resolve({
								ok: true,
								status: 200,
								json: async () => ({ warnings: [] }),
							} as Response);
					}),
			);
			const screen = await renderValidSurvey(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			const respondentInput = screen.getByPlaceholder("Pragmatic Developers");

			await screen.getByRole("button", { name: /Analyse Survey/i }).click();

			await expect.element(respondentInput).toBeDisabled();
			completeAnalysis?.();
			await expect.element(respondentInput).not.toBeDisabled();
		});

		it("should analyze survey data and show semantic suggestions", async () => {
			const screen = await renderValidSurvey(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			mockApiCall.mockResolvedValue({
				ok: true,
				status: 200,
				json: async () => ({
					responseShape: "single_choice",
					responseShapeConfidence: 0.95,
					leadingProbability: 0.9,
					multipleChoiceProbability: 0.1,
					coverageProbability: 0.9,
					warnings: [
						{
							code: "question.leading",
							title: "Survey Has Already Voted",
							message:
								"This question appears to have a favourite. Try not to make respondents agree under oath.",
							probability: 0.9,
						},
					],
				}),
			});

			await screen.getByRole("button", { name: /Analyse Survey/i }).click();

			await expect
				.poll(() => mockApiCall)
				.toHaveBeenCalledWith("api/survey/analyze", {
					method: "POST",
					body: expect.stringContaining("surveyTopic"),
				});
			await expect
				.element(
					screen.getByText(
						"This question appears to have a favourite. Try not to make respondents agree under oath.",
					),
				)
				.toBeInTheDocument();
			await expect
				.element(screen.getByText("Survey Has Already Voted"))
				.toBeInTheDocument();
			const analyzeButton = screen.getByRole("button", {
				name: "Analyse Survey",
			});
			await expect.element(analyzeButton).toBeDisabled();

			await screen
				.getByPlaceholder("Do you prefer tabs or spaces?")
				.fill("A different question");
			await expect.element(analyzeButton).not.toBeDisabled();
		});

		it("should clear analysis warnings after creating a survey", async () => {
			const screen = await renderValidSurvey(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			mockApiCall
				.mockResolvedValueOnce({
					ok: true,
					status: 200,
					json: async () => ({
						warnings: [
							{
								code: "question.leading",
								title: "Survey Has Already Voted",
								message: "This question has already picked a side.",
								probability: 0.9,
							},
						],
					}),
				})
				.mockResolvedValueOnce({
					ok: true,
					status: 201,
					json: async () => ({ id: 123, topic: "Test Survey" }),
				});

			await screen.getByRole("button", { name: "Analyse Survey" }).click();
			await expect
				.element(screen.getByText("Survey Has Already Voted"))
				.toBeInTheDocument();
			const createButton = screen.getByRole("button", {
				name: "Create Survey (Despite All The Issues Identified)",
			});
			await expect.element(createButton).toBeDisabled();
			const confirmation = screen.getByLabelText(
				"I confirm that I want to create this survey with bad data & that I feel bad about it",
			);
			await confirmation.click();
			await expect.element(createButton).not.toBeDisabled();
			await createButton.click();

			await expect
				.element(screen.getByText("Survey Has Already Voted"))
				.not.toBeInTheDocument();
			await expect
				.element(screen.getByRole("button", { name: "Create Survey" }))
				.toBeInTheDocument();
		});
	});

	describe("Form Submission", () => {
		it("should submit form with survey data", async () => {
			const screen = await renderValidSurvey(
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
			const screen = await renderValidSurvey(
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
			const screen = await renderValidSurvey(
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

		it("should keep the analysis button disabled after successful submission", async () => {
			const screen = await renderValidSurvey(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			mockApiCall.mockResolvedValue({
				ok: true,
				status: 201,
				json: async () => ({ id: 999, topic: "Test Survey" }),
			});

			await screen.getByRole("button", { name: /Create Survey/i }).click();

			await expect
				.element(screen.getByRole("button", { name: "Analyse Survey" }))
				.toBeDisabled();
		});

		it("should clear stale success and allow analysis after editing a retained successful draft", async () => {
			const screen = await renderValidSurvey(
				<CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />,
			);
			const question = screen.getByPlaceholder("Do you prefer tabs or spaces?");
			await question.fill("Tabs or spaces?");
			await screen
				.getByRole("button", { name: "Create Survey", exact: true })
				.click();

			await expect
				.element(screen.getByText(/Survey created with ID: 123/))
				.toBeInTheDocument();
			await expect
				.element(screen.getByRole("button", { name: "Analyse Survey" }))
				.toBeDisabled();

			await question.fill("Tea or coffee?");

			await expect
				.element(screen.getByText(/Survey created with ID: 123/))
				.not.toBeInTheDocument();
			await expect
				.element(screen.getByRole("button", { name: "Analyse Survey" }))
				.not.toBeDisabled();
			await expect.element(question).toHaveValue("Tea or coffee?");
		});

		it("should show error message on failed submission", async () => {
			const screen = await renderValidSurvey(
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
			const screen = await renderValidSurvey(
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
				.element(
					screen.getByRole("link", { name: "Question: Topic is required" }),
				)
				.toBeInTheDocument();
			await expect
				.element(
					screen.getByRole("link", {
						name: "Number of Respondents: Must be greater than 0",
					}),
				)
				.toBeInTheDocument();
			await expect
				.element(screen.getByRole("region", { name: "Check survey details" }))
				.toHaveFocus();
			await screen
				.getByRole("link", { name: "Question: Topic is required" })
				.click();
			const question = screen.getByRole("textbox", {
				name: "Question (Survey Topic)",
			});
			await expect.element(question).toHaveFocus();
			await expect.element(question).toHaveAttribute("aria-invalid", "true");
			await expect
				.element(question)
				.toHaveAccessibleDescription("Error: Topic is required");
			await question.fill("Updated question");
			await expect.element(question).toHaveAttribute("aria-invalid", "false");
		});

		it("should show duplicate option error on 422 domain exception response", async () => {
			const screen = await renderValidSurvey(
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
			const screen = await renderValidSurvey(
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
			const screen = await renderValidSurvey(
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
			const screen = await renderValidSurvey(
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

	describe("Accessible validation and option editing", () => {
		it.each([800, 320])(
			"aligns remove buttons with their vote inputs at %spx",
			async (width) => {
				const previous = {
					width: window.innerWidth,
					height: window.innerHeight,
				};
				await page.viewport(width, 900);
				try {
					const screen = await render(<CreateSurvey loading={false} />);
					await screen.getByRole("button", { name: "Add Option" }).click();
					await expect
						.element(screen.getByRole("textbox", { name: "#2", exact: true }))
						.toBeInTheDocument();
					const votes = screen.getByRole("spinbutton", {
						name: "Preferred Votes for #2",
					});
					const input = votes.element();
					const button = screen
						.getByRole("button", { name: "Remove #2", exact: true })
						.element();
					const assertAligned = () => {
						const inputBounds = input.getBoundingClientRect();
						const buttonBounds = button.getBoundingClientRect();
						expect(
							Math.abs(
								(inputBounds.top + inputBounds.bottom) / 2 -
									(buttonBounds.top + buttonBounds.bottom) / 2,
							),
						).toBeLessThanOrEqual(1);
					};
					assertAligned();
					await votes.fill("-1");
					await screen
						.getByRole("textbox", { name: "#2", exact: true })
						.fill("Another option");
					await expect.element(votes).toHaveAttribute("aria-invalid", "true");
					assertAligned();
				} finally {
					await page.viewport(previous.width, previous.height);
				}
			},
		);

		it("replaces failed analysis feedback with the current creation errors", async () => {
			const screen = await renderValidSurvey(<CreateSurvey loading={false} />);
			mockApiCall.mockResolvedValueOnce({
				ok: false,
				status: 422,
				json: async () => ({
					SurveyTopic: ["Analysis rejected this question"],
				}),
			});
			mockApiCall.mockResolvedValueOnce({
				ok: false,
				status: 422,
				json: async () => ({
					NumberOfRespondents: ["Creation rejected this count"],
				}),
			});
			await screen.getByRole("button", { name: "Analyse Survey" }).click();
			await expect
				.element(
					screen.getByRole("link", {
						name: "Question: Analysis rejected this question",
					}),
				)
				.toBeInTheDocument();
			await screen
				.getByRole("button", { name: "Create Survey", exact: true })
				.click();
			await expect
				.element(
					screen.getByRole("link", {
						name: "Number of Respondents: Creation rejected this count",
					}),
				)
				.toBeInTheDocument();
			await expect
				.element(
					screen.getByRole("link", {
						name: "Question: Analysis rejected this question",
					}),
				)
				.not.toBeInTheDocument();
		});

		it("keeps form controls within a narrow viewport", async () => {
			const previous = { width: window.innerWidth, height: window.innerHeight };
			await page.viewport(320, 800);
			try {
				const screen = await render(<CreateSurvey loading={false} />);
				await screen.getByRole("button", { name: "Add Option" }).click();
				await expect
					.element(screen.getByRole("textbox", { name: "#2", exact: true }))
					.toBeInTheDocument();
				expect(document.documentElement.scrollWidth).toBeLessThanOrEqual(
					window.innerWidth,
				);
				for (const control of screen.container.querySelectorAll(
					"input, button",
				)) {
					const bounds = control.getBoundingClientRect();
					expect(bounds.left).toBeGreaterThanOrEqual(0);
					expect(bounds.right).toBeLessThanOrEqual(window.innerWidth);
				}
			} finally {
				await page.viewport(previous.width, previous.height);
			}
		});

		it.each(["-1", "1.5", "101"])(
			"rejects invalid preferred votes %s without truncating the value",
			async (value) => {
				const screen = await renderValidSurvey(
					<CreateSurvey loading={false} />,
				);
				const votes = screen.getByRole("spinbutton", {
					name: "Preferred Votes for #1",
				});
				await votes.fill(value);
				await screen
					.getByRole("button", { name: "Create Survey", exact: true })
					.click();
				expect(mockApiCall).not.toHaveBeenCalled();
				await expect.element(votes).toHaveFocus();
				await expect.element(votes).toHaveValue(Number(value));
			},
		);

		it.each(["", "0", "-1", "1.5", "1000001"])(
			"blocks creation with respondent count %s and focuses the invalid field",
			async (value) => {
				const screen = await renderValidSurvey(
					<CreateSurvey loading={false} />,
				);
				const count = screen.getByRole("spinbutton", {
					name: "Number of Respondents",
				});
				await count.fill(value);
				await screen
					.getByRole("button", { name: "Create Survey", exact: true })
					.click();
				expect(mockApiCall).not.toHaveBeenCalled();
				await expect.element(count).toHaveFocus();
				await expect.element(count).toHaveAttribute("aria-invalid", "true");
			},
		);

		it("blocks analysis of missing text and focuses the first invalid field", async () => {
			const screen = await render(<CreateSurvey loading={false} />);
			const audience = screen.getByRole("textbox", {
				name: "Target Audience (Respondent Type)",
			});
			await expect.element(audience).toHaveAttribute("aria-invalid", "false");
			await screen.getByRole("button", { name: "Analyse Survey" }).click();
			expect(mockApiCall).not.toHaveBeenCalled();
			await expect.element(audience).toHaveFocus();
			await expect.element(audience).toHaveAttribute("aria-invalid", "true");
		});

		it("analyses valid text without requiring a respondent count or vote allocation", async () => {
			const screen = await renderValidSurvey(<CreateSurvey loading={false} />);
			await screen
				.getByRole("spinbutton", { name: "Number of Respondents" })
				.fill("");
			await screen
				.getByRole("spinbutton", { name: "Preferred Votes for #1" })
				.fill("-1");
			mockApiCall.mockResolvedValue({
				ok: true,
				status: 200,
				json: async () => ({ warnings: [] }),
			});
			await screen.getByRole("button", { name: "Analyse Survey" }).click();
			await expect.poll(() => mockApiCall).toHaveBeenCalledTimes(1);
			expect(mockApiCall.mock.calls[0][0]).toBe("api/survey/analyze");
			expect(JSON.parse(mockApiCall.mock.calls[0][1].body)).not.toHaveProperty(
				"numberOfRespondents",
			);
		});

		it("limits analysis to 20 options while allowing creation of a larger valid survey", async () => {
			const screen = await renderValidSurvey(<CreateSurvey loading={false} />);
			for (let number = 2; number <= 21; number++) {
				await screen.getByRole("button", { name: "Add Option" }).click();
				await screen
					.getByRole("textbox", { name: `#${number}`, exact: true })
					.fill(`Option ${number}`);
			}
			await screen.getByRole("button", { name: "Analyse Survey" }).click();
			expect(mockApiCall).not.toHaveBeenCalled();
			await expect
				.element(screen.getByRole("region", { name: "Check survey details" }))
				.toHaveFocus();
			await screen
				.getByRole("button", { name: "Create Survey", exact: true })
				.click();
			await expect.poll(() => mockApiCall).toHaveBeenCalledTimes(1);
			expect(mockApiCall.mock.calls[0][0]).toBe("api/survey");
			await expect
				.element(screen.getByRole("region", { name: "Check survey details" }))
				.not.toBeInTheDocument();
		});

		it("focuses new options and adjacent surviving inputs after removal", async () => {
			const screen = await render(<CreateSurvey loading={false} />);
			await screen.getByRole("button", { name: "Add Option" }).click();
			await expect
				.element(screen.getByRole("textbox", { name: "#2", exact: true }))
				.toHaveFocus();
			await screen.getByRole("button", { name: "Add Option" }).click();
			const survivingInput = screen.container.querySelector<HTMLInputElement>(
				'input[name="SurveyOptions[2].OptionText"]',
			);
			await expect
				.element(screen.getByRole("textbox", { name: "#3", exact: true }))
				.toHaveFocus();
			await screen
				.getByRole("button", { name: "Remove #2", exact: true })
				.click();
			expect(document.activeElement).toBe(survivingInput);
			await screen
				.getByRole("button", { name: "Remove #2", exact: true })
				.click();
			await expect
				.element(screen.getByRole("textbox", { name: "#1", exact: true }))
				.toHaveFocus();
		});

		it("associates indexed server errors with the correct stable option input", async () => {
			const screen = await renderValidSurvey(<CreateSurvey loading={false} />);
			await screen.getByRole("button", { name: "Add Option" }).click();
			const option = screen.getByRole("textbox", { name: "#2", exact: true });
			await option.fill("Another option");
			mockApiCall.mockResolvedValue({
				ok: false,
				status: 422,
				json: async () => ({
					"SurveyOptions[1].OptionText": ["Use distinct option text"],
				}),
			});
			await screen
				.getByRole("button", { name: "Create Survey", exact: true })
				.click();
			await screen
				.getByRole("link", { name: "Option #2: Use distinct option text" })
				.click();
			await expect.element(option).toHaveFocus();
			await expect
				.element(option)
				.toHaveAccessibleDescription("Error: Use distinct option text");
		});
	});
});
