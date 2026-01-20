import "@testing-library/jest-dom";
import { vi } from "vitest";

// Mock Auth0 provider
vi.mock("@auth0/auth0-react", () => ({
  useAuth0: vi.fn(() => ({
    isAuthenticated: true,
    isLoading: false,
    user: { sub: "test-user-id", name: "Test User" },
    getAccessTokenSilently: vi.fn(async () => "test-token"),
    loginWithRedirect: vi.fn(),
    logout: vi.fn(),
  })),
  Auth0Provider: ({ children }: { children: React.ReactNode }) => children,
}));

// Mock window.matchMedia
Object.defineProperty(window, "matchMedia", {
  writable: true,
  value: vi.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});
