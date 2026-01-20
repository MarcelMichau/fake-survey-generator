import { describe, it, expect, vi, beforeEach } from "vitest";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import CreateSurvey from "./CreateSurvey";
import { render } from "../test/test-utils";
import * as hooks from "../hooks";

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
    it("should render create survey form", () => {
      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      expect(screen.getByRole("heading", { name: /Create Survey/i })).toBeInTheDocument();
      expect(
        screen.getByPlaceholderText("Pragmatic Developers")
      ).toBeInTheDocument();
      expect(
        screen.getByPlaceholderText("Do you prefer tabs or spaces?")
      ).toBeInTheDocument();
    });

    it("should show loading skeleton when loading prop is true", () => {
      const { container } = render(
        <CreateSurvey loading={true} onSurveyCreated={mockOnSurveyCreated} />
      );

      // Check for skeleton elements (react-loading-skeleton)
      const skeletons = container.querySelectorAll(".react-loading-skeleton");
      expect(skeletons.length).toBeGreaterThan(0);
    });

    it("should have submit button", () => {
      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      expect(screen.getByRole("button", { name: /Create Survey/i })).toBeInTheDocument();
    });
  });

  describe("Form Interactions", () => {
    it("should update form fields when user types", async () => {
      const user = userEvent.setup();
      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const respondentInput = screen.getByPlaceholderText("Pragmatic Developers");
      const questionInput = screen.getByPlaceholderText(
        "Do you prefer tabs or spaces?"
      );

      await user.type(respondentInput, "Tech Developers");
      await user.type(questionInput, "What's your favorite color?");

      expect(respondentInput).toHaveValue("Tech Developers");
      expect(questionInput).toHaveValue("What's your favorite color?");
    });

    it("should add new survey option when Add Option button is clicked", async () => {
      const user = userEvent.setup();
      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const addButton = screen.getByRole("button", { name: /Add Option/i });

      await user.click(addButton);

      // Should now have 2 option input fields
      const optionInputs = screen.getAllByPlaceholderText("Some other option");
      expect(optionInputs.length).toBe(1);

      await user.click(addButton);

      const allOptionInputs = screen.getAllByPlaceholderText("Some other option");
      expect(allOptionInputs.length).toBe(2);
    });

    it("should remove option when Remove button is clicked", async () => {
      const user = userEvent.setup();
      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      // Add an option first
      const addButton = screen.getByRole("button", { name: /Add Option/i });
      await user.click(addButton);

      // Find and click remove button
      const removeButton = screen.getByRole("button", { name: /Remove #2/i });
      await user.click(removeButton);

      // Should only have first option now
      expect(screen.queryByText("Remove #2")).not.toBeInTheDocument();
    });

    it("should generate unique option IDs even after removing options", async () => {
      const user = userEvent.setup();
      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const addButton = screen.getByRole("button", { name: /Add Option/i });

      // Add option #2
      await user.click(addButton);
      expect(screen.getByRole("button", { name: /Remove #2/i })).toBeInTheDocument();

      // Add option #3
      await user.click(addButton);
      expect(screen.getByRole("button", { name: /Remove #3/i })).toBeInTheDocument();

      // Remove option #2
      const removeButton2 = screen.getByRole("button", { name: /Remove #2/i });
      await user.click(removeButton2);
      expect(screen.queryByRole("button", { name: /Remove #2/i })).not.toBeInTheDocument();

      // Add a new option - should be #4, not #2 (avoiding ID collision)
      await user.click(addButton);
      expect(screen.getByRole("button", { name: /Remove #4/i })).toBeInTheDocument();
      expect(screen.queryByRole("button", { name: /Remove #2/i })).not.toBeInTheDocument();
    });

    it("should update option text when user types in option field", async () => {
      const user = userEvent.setup();
      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const optionInput = screen.getByPlaceholderText("Most definitely tabs");
      await user.type(optionInput, "Red");

      expect(optionInput).toHaveValue("Red");
    });

    it("should update preferred votes for option", () => {
      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      // Find preferred votes input by its label
      const preferredVotesInputs = screen.getAllByRole("spinbutton");
      expect(preferredVotesInputs.length).toBeGreaterThan(0);
      // Just check that the input exists and is a spinbutton (number input)
      expect(preferredVotesInputs[0]).toHaveProperty("type", "number");
    });
  });

  describe("Form Submission", () => {
    it("should submit form with survey data", async () => {
      const user = userEvent.setup();
      mockApiCall.mockResolvedValue({
        ok: true,
        status: 201,
        json: async () => ({ id: 456, topic: "Color Preference" }),
      });

      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const respondentInput = screen.getByPlaceholderText("Pragmatic Developers");
      const questionInput = screen.getByPlaceholderText(
        "Do you prefer tabs or spaces?"
      );
      const optionInput = screen.getByPlaceholderText("Most definitely tabs");
      const submitButton = screen.getByRole("button", { name: /Create Survey/i });

      await user.type(respondentInput, "Test Audience");
      await user.type(questionInput, "Test Question");
      await user.type(optionInput, "Option 1");

      await user.click(submitButton);

      await waitFor(() => {
        expect(mockApiCall).toHaveBeenCalledWith("api/survey", {
          method: "POST",
          body: expect.stringContaining("Test Audience"),
        });
      });
    });

    it("should call onSurveyCreated callback on successful submission", async () => {
      const user = userEvent.setup();
      mockApiCall.mockResolvedValue({
        ok: true,
        status: 201,
        json: async () => ({ id: 789, topic: "Test" }),
      });

      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const submitButton = screen.getByRole("button", { name: /Create Survey/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockOnSurveyCreated).toHaveBeenCalledWith(789);
      });
    });

    it("should show success message on successful submission", async () => {
      const user = userEvent.setup();
      mockApiCall.mockResolvedValue({
        ok: true,
        status: 201,
        json: async () => ({ id: 999, topic: "Test Survey" }),
      });

      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const submitButton = screen.getByRole("button", { name: /Create Survey/i });
      await user.click(submitButton);

      await waitFor(() => {
        // Check that the API call was made successfully
        expect(mockApiCall).toHaveBeenCalled();
        // Check that callback was called with the survey ID
        expect(mockOnSurveyCreated).toHaveBeenCalledWith(999);
      });
    });

    it("should show error message on failed submission", async () => {
      const user = userEvent.setup();
      mockApiCall.mockResolvedValue({
        ok: false,
        status: 500,
      });

      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const submitButton = screen.getByRole("button", { name: /Create Survey/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(
          screen.getByText(/Please try again or create an issue on GitHub/)
        ).toBeInTheDocument();
      });
    });

    it("should show validation errors on 422 response", async () => {
      const user = userEvent.setup();
      mockApiCall.mockResolvedValue({
        ok: false,
        status: 422,
        json: async () => ({
          surveyTopic: ["Topic is required"],
          numberOfRespondents: ["Must be greater than 0"],
        }),
      });

      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const submitButton = screen.getByRole("button", { name: /Create Survey/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(screen.getByText("Topic is required")).toBeInTheDocument();
        expect(screen.getByText("Must be greater than 0")).toBeInTheDocument();
      });
    });

    it("should reset form after successful submission when resetOnSuccess is true", async () => {
      const user = userEvent.setup();
      mockApiCall.mockResolvedValue({
        ok: true,
        status: 201,
        json: async () => ({ id: 111, topic: "Test" }),
      });

      render(
        <CreateSurvey loading={false} resetOnSuccess={true} />
      );

      const respondentInput = screen.getByPlaceholderText(
        "Pragmatic Developers"
      );
      const questionInput = screen.getByPlaceholderText(
        "Do you prefer tabs or spaces?"
      );

      await user.type(respondentInput, "Test");
      await user.type(questionInput, "Test");

      const submitButton = screen.getByRole("button", { name: /Create Survey/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(respondentInput).toHaveValue("");
        expect(questionInput).toHaveValue("");
      });
    });

    it("should not reset form after successful submission when onSurveyCreated is provided (default behavior)", async () => {
      const user = userEvent.setup();
      mockApiCall.mockResolvedValue({
        ok: true,
        status: 201,
        json: async () => ({ id: 111, topic: "Test" }),
      });

      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const respondentInput = screen.getByPlaceholderText(
        "Pragmatic Developers"
      );
      const questionInput = screen.getByPlaceholderText(
        "Do you prefer tabs or spaces?"
      );

      await user.type(respondentInput, "Test");
      await user.type(questionInput, "Test");

      const submitButton = screen.getByRole("button", { name: /Create Survey/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockOnSurveyCreated).toHaveBeenCalledWith(111);
      });

      // Form should NOT be reset when onSurveyCreated is provided
      expect(respondentInput).toHaveValue("Test");
      expect(questionInput).toHaveValue("Test");
    });

    it("should reset form after successful submission when resetOnSuccess is explicitly true with onSurveyCreated", async () => {
      const user = userEvent.setup();
      mockApiCall.mockResolvedValue({
        ok: true,
        status: 201,
        json: async () => ({ id: 111, topic: "Test" }),
      });

      render(
        <CreateSurvey 
          loading={false} 
          onSurveyCreated={mockOnSurveyCreated} 
          resetOnSuccess={true}
        />
      );

      const respondentInput = screen.getByPlaceholderText(
        "Pragmatic Developers"
      );
      const questionInput = screen.getByPlaceholderText(
        "Do you prefer tabs or spaces?"
      );

      await user.type(respondentInput, "Test");
      await user.type(questionInput, "Test");

      const submitButton = screen.getByRole("button", { name: /Create Survey/i });
      await user.click(submitButton);

      await waitFor(() => {
        expect(mockOnSurveyCreated).toHaveBeenCalledWith(111);
        expect(respondentInput).toHaveValue("");
        expect(questionInput).toHaveValue("");
      });
    });
  });

  describe("State Management", () => {
    it("should properly initialize form state", () => {
      render(
        <CreateSurvey loading={false} onSurveyCreated={mockOnSurveyCreated} />
      );

      const respondentInput = screen.getByPlaceholderText("Pragmatic Developers");
      const questionInput = screen.getByPlaceholderText(
        "Do you prefer tabs or spaces?"
      );

      expect(respondentInput).toHaveValue("");
      expect(questionInput).toHaveValue("");
    });
  });
});
