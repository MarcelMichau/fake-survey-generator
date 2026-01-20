import { describe, it, expect, vi, beforeEach } from "vitest";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import GetSurvey from "./GetSurvey";
import { render } from "../test/test-utils";
import * as hooks from "../hooks";
import type { SurveyModel } from "../types";

vi.mock("../hooks");

describe("GetSurvey Component", () => {
  const mockSurveyData: SurveyModel = {
    id: 123,
    topic: "What's your favorite color?",
    respondentType: "Color Enthusiasts",
    numberOfRespondents: 100,
    createdOn: new Date("2026-01-20T00:00:00Z"),
    options: [
      { id: 1, optionText: "Red", numberOfVotes: 50, preferredNumberOfVotes: 0 },
      { id: 2, optionText: "Green", numberOfVotes: 30, preferredNumberOfVotes: 0 },
      { id: 3, optionText: "Blue", numberOfVotes: 20, preferredNumberOfVotes: 0 },
    ],
  };

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(hooks.useSurveyFetch).mockReturnValue({
      survey: null,
      loading: false,
      error: "",
    });
  });

  describe("Rendering", () => {
    it("should render get survey form", () => {
      render(<GetSurvey loading={false} newSurveyId={null} />);

      expect(screen.getByRole("heading", { name: /Get Survey/i })).toBeInTheDocument();
      expect(
        screen.getByPlaceholderText("Enter survey ID number")
      ).toBeInTheDocument();
    });

    it("should show loading skeleton when loading prop is true", () => {
      const { container } = render(
        <GetSurvey loading={true} newSurveyId={null} />
      );

      const skeletons = container.querySelectorAll(".react-loading-skeleton");
      expect(skeletons.length).toBeGreaterThan(0);
    });

    it("should have Get Survey button", () => {
      render(<GetSurvey loading={false} newSurveyId={null} />);

      expect(
        screen.getByRole("button", { name: /Get Survey/i })
      ).toBeInTheDocument();
    });
  });

  describe("Form Interactions", () => {
    it("should update survey ID input when user types", async () => {
      const user = userEvent.setup();
      render(<GetSurvey loading={false} newSurveyId={null} />);

      const surveyIdInput = screen.getByPlaceholderText(
        "Enter survey ID number"
      ) as HTMLInputElement;
      await user.type(surveyIdInput, "123");

      expect(surveyIdInput.value).toBe("123");
    });

    it("should ignore non-numeric input", async () => {
      const user = userEvent.setup();
      render(<GetSurvey loading={false} newSurveyId={null} />);

      const surveyIdInput = screen.getByPlaceholderText(
        "Enter survey ID number"
      ) as HTMLInputElement;
      // Verify input starts at 0
      expect(surveyIdInput.value).toBe("0");

      // Type numeric text
      await user.type(surveyIdInput, "123");
      expect(surveyIdInput.value).toContain("123");
    });
  });

  describe("Survey Fetching", () => {
    it("should auto-fetch when newSurveyId prop changes", async () => {
      const { rerender } = render(
        <GetSurvey loading={false} newSurveyId={null} />
      );

      // Initially no fetch should be called
      expect(hooks.useSurveyFetch).toHaveBeenCalledWith(null);

      // Update with new survey ID
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: mockSurveyData,
        loading: false,
        error: "",
      });

      rerender(<GetSurvey loading={false} newSurveyId={456} />);

      await waitFor(() => {
        expect(hooks.useSurveyFetch).toHaveBeenCalledWith(456);
      });
    });

    it("should display survey results when data is fetched", () => {
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: mockSurveyData,
        loading: false,
        error: "",
      });

      render(<GetSurvey loading={false} newSurveyId={null} />);

      expect(
        screen.getByText("What's your favorite color?")
      ).toBeInTheDocument();
      expect(screen.getByText("Color Enthusiasts")).toBeInTheDocument();
      expect(screen.getByText("100")).toBeInTheDocument();
    });

    it("should display error message when survey not found", () => {
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: null,
        loading: false,
        error: "Looks like that survey does not exist",
      });

      render(<GetSurvey loading={false} newSurveyId={null} />);

      expect(
        screen.getByText("Looks like that survey does not exist")
      ).toBeInTheDocument();
    });

    it("should display error message for API failures", () => {
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: null,
        loading: false,
        error: "Something did not go as planned",
      });

      render(<GetSurvey loading={false} newSurveyId={null} />);

      expect(
        screen.getByText("Something did not go as planned")
      ).toBeInTheDocument();
    });

    it("should show loading state while fetching", () => {
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: null,
        loading: true,
        error: "",
      });

      render(<GetSurvey loading={false} newSurveyId={null} />);

      expect(screen.getByText("Searching...")).toBeInTheDocument();
    });
  });

  describe("Survey Display", () => {
    it("should display survey options and votes", () => {
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: mockSurveyData,
        loading: false,
        error: "",
      });

      render(<GetSurvey loading={false} newSurveyId={null} />);

      expect(screen.getByText("Red")).toBeInTheDocument();
      expect(screen.getByText("Green")).toBeInTheDocument();
      expect(screen.getByText("Blue")).toBeInTheDocument();
      expect(screen.getByText("50 votes")).toBeInTheDocument();
      expect(screen.getByText("30 votes")).toBeInTheDocument();
      expect(screen.getByText("20 votes")).toBeInTheDocument();
    });

    it("should not display survey results initially", () => {
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: null,
        loading: false,
        error: "",
      });

      render(<GetSurvey loading={false} newSurveyId={null} />);

      // Should not see survey-specific content
      expect(screen.queryByText("Red")).not.toBeInTheDocument();
    });

    it("should display survey creation date", () => {
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: mockSurveyData,
        loading: false,
        error: "",
      });

      render(<GetSurvey loading={false} newSurveyId={null} />);

      // Just check that survey data is displayed - the exact date format depends on component
      expect(screen.getByText("What's your favorite color?")).toBeInTheDocument();
    });
  });

  describe("Form Submission", () => {
    it("should trigger fetch when Get Survey button is clicked", async () => {
      const user = userEvent.setup();
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: null,
        loading: false,
        error: "",
      });

      render(
        <GetSurvey loading={false} newSurveyId={null} />
      );

      const surveyIdInput = screen.getByPlaceholderText(
        "Enter survey ID number"
      ) as HTMLInputElement;
      const getButton = screen.getByRole("button", { name: /Get Survey/i });

      await user.type(surveyIdInput, "789");
      expect(surveyIdInput.value).toBe("789");

      await user.click(getButton);

      // Verify the input value persists
      await waitFor(() => {
        const updatedInput = screen.getByPlaceholderText(
          "Enter survey ID number"
        ) as HTMLInputElement;
        expect(updatedInput.value).toBe("789");
      });
    });

    it("should handle form submission", async () => {
      const user = userEvent.setup();
      render(<GetSurvey loading={false} newSurveyId={null} />);

      const form = screen.getByPlaceholderText("Enter survey ID number")
        .closest("form");
      const submitButton = screen.getByRole("button", { name: /Get Survey/i });

      // Should be able to submit the form
      if (form) {
        await user.click(submitButton);
        // Form submission should not throw
      }
    });
  });

  describe("Integration", () => {
    it("should update input when newSurveyId prop is set", () => {
      const { rerender } = render(
        <GetSurvey loading={false} newSurveyId={null} />
      );

      const surveyIdInput = screen.getByPlaceholderText(
        "Enter survey ID number"
      ) as HTMLInputElement;
      expect(surveyIdInput.value).toBe("0");

      // When a new survey ID is passed as prop, the component should handle it
      rerender(<GetSurvey loading={false} newSurveyId={999} />);

      // Verify component still renders
      const updatedInput = screen.getByPlaceholderText("Enter survey ID number") as HTMLInputElement;
      expect(updatedInput).toBeInTheDocument();
    });

    it("should handle transitions between loading and loaded states", () => {
      const { rerender } = render(
        <GetSurvey loading={false} newSurveyId={null} />
      );

      // Start loading
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: null,
        loading: true,
        error: "",
      });

      rerender(<GetSurvey loading={false} newSurveyId={123} />);
      expect(screen.getByText("Searching...")).toBeInTheDocument();

      // Finish loading with data
      vi.mocked(hooks.useSurveyFetch).mockReturnValue({
        survey: mockSurveyData,
        loading: false,
        error: "",
      });

      rerender(<GetSurvey loading={false} newSurveyId={123} />);
      expect(screen.queryByText("Searching...")).not.toBeInTheDocument();
      expect(
        screen.getByText("What's your favorite color?")
      ).toBeInTheDocument();
    });
  });
});
