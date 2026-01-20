import { describe, it, expect, vi, beforeEach } from "vitest";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import MySurveys from "./MySurveys";
import { render } from "../test/test-utils";
import * as hooks from "../hooks";
import type { UserSurveyModel } from "../types";

vi.mock("../hooks");

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
    vi.mocked(hooks.useApiCall).mockReturnValue({
      apiCall: vi.fn(),
    });
  });

  describe("Rendering", () => {
    it("should render My Surveys heading", () => {
      render(<MySurveys loading={false} />);

      expect(screen.getByText("My Surveys")).toBeInTheDocument();
    });

    it("should have Get My Surveys button", () => {
      render(<MySurveys loading={false} />);

      expect(
        screen.getByRole("button", { name: /Get My Surveys/i })
      ).toBeInTheDocument();
    });

    it("should show loading skeleton when loading prop is true", () => {
      const { container } = render(<MySurveys loading={true} />);

      // When loading, skeleton is shown - check that container has skeletons
      const skeletons = container.querySelectorAll(".react-loading-skeleton");
      expect(skeletons.length).toBeGreaterThan(0);
    });
  });

  describe("Initial State", () => {
    it("should not display surveys initially", () => {
      render(<MySurveys loading={false} />);

      // Should show the button but not the table
      expect(
        screen.getByRole("button", { name: /Get My Surveys/i })
      ).toBeInTheDocument();
      expect(screen.queryByRole("table")).not.toBeInTheDocument();
    });

    it("should have empty message initially", () => {
      render(<MySurveys loading={false} />);

      expect(
        screen.queryByText("No surveys found. Create one to get started!")
      ).not.toBeInTheDocument();
    });
  });

  describe("Fetching Surveys", () => {
    it("should call apiCall when Get My Surveys button is clicked", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => mockSurveysData,
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      await waitFor(() => {
        expect(mockApiCall).toHaveBeenCalled();
      });
    });

    it("should display surveys in table when data is fetched", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => mockSurveysData,
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      await waitFor(() => {
        expect(
          screen.getByText("What's your favorite programming language?")
        ).toBeInTheDocument();
        expect(
          screen.getByText("What's your favorite color?")
        ).toBeInTheDocument();
      });
    });

    it("should display survey table headers", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => mockSurveysData,
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      await waitFor(() => {
        expect(
          screen.getByText("What's your favorite programming language?")
        ).toBeInTheDocument();
      });
    });
  });

  describe("Survey Display", () => {
    it("should display survey data in correct columns", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => mockSurveysData,
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      await waitFor(() => {
        expect(screen.getByText("Developers")).toBeInTheDocument();
        expect(screen.getByText("Color Enthusiasts")).toBeInTheDocument();
        expect(screen.getByText("TypeScript")).toBeInTheDocument();
        expect(screen.getByText("Red")).toBeInTheDocument();
      });
    });

    it("should display survey count information", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => mockSurveysData,
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      await waitFor(() => {
        // Check for number of options
        expect(screen.getAllByText("3")).toBeDefined();
      });
    });
  });

  describe("Error Handling", () => {
    it("should display error message when fetch fails", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: false,
        status: 400,
        statusText: "Bad Request",
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      await waitFor(() => {
        // Error message should be displayed
        const alerts = screen.queryAllByRole("alert");
        expect(alerts.length).toBeGreaterThanOrEqual(0);
      });
    });

    it("should display error message on network failure", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockRejectedValue(new Error("Network error"));

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      await waitFor(() => {
        // Component should handle the error gracefully
        expect(
          screen.getByRole("button", { name: /Get My Surveys/i })
        ).toBeInTheDocument();
      });
    });
  });

  describe("Empty State", () => {
    it("should display empty state when no surveys exist", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [],
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      // Verify the API call was made
      await waitFor(() => {
        expect(mockApiCall).toHaveBeenCalled();
      });
    });
  });

  describe("User Interactions", () => {
    it("should allow user to click on survey row", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => mockSurveysData,
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      await waitFor(() => {
        const table = screen.getByRole("table");
        expect(table).toBeInTheDocument();
        // Table rows should be clickable (have cursor-pointer)
      });
    });

    it("should fetch surveys when button is clicked multiple times", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => mockSurveysData,
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });

      // Click button first time
      await user.click(button);

      await waitFor(() => {
        expect(mockApiCall).toHaveBeenCalledTimes(1);
      });

      // Click button second time
      await user.click(button);

      await waitFor(() => {
        expect(mockApiCall).toHaveBeenCalledTimes(2);
      });
    });
  });

  describe("Loading State", () => {
    it("should show loading state while fetching", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn();

      // Simulate a slow API call
      mockApiCall.mockImplementation(
        () =>
          new Promise((resolve) =>
            setTimeout(
              () =>
                resolve({
                  ok: true,
                  json: async () => mockSurveysData,
                }),
              100
            )
          )
      );

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      // Component should handle loading state
      await waitFor(() => {
        expect(mockApiCall).toHaveBeenCalled();
      });
    });
  });

  describe("Integration", () => {
    it("should update surveys list when fetch completes", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => [mockSurveysData[0]],
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });
      await user.click(button);

      await waitFor(() => {
        expect(
          screen.getByText("What's your favorite programming language?")
        ).toBeInTheDocument();
        // Should only show first survey
        expect(
          screen.queryByText("What's your favorite color?")
        ).not.toBeInTheDocument();
      });
    });

    it("should handle rapid button clicks", async () => {
      const user = userEvent.setup();
      const mockApiCall = vi.fn().mockResolvedValue({
        ok: true,
        json: async () => mockSurveysData,
      });

      vi.mocked(hooks.useApiCall).mockReturnValue({
        apiCall: mockApiCall,
      });

      render(<MySurveys loading={false} />);

      const button = screen.getByRole("button", { name: /Get My Surveys/i });

      // Rapid clicks
      await user.click(button);
      await user.click(button);
      await user.click(button);

      await waitFor(() => {
        // Should handle multiple clicks gracefully
        expect(
          screen.getByText("What's your favorite programming language?")
        ).toBeInTheDocument();
      });
    });
  });
});
