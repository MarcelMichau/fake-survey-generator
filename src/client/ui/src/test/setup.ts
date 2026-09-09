import "vitest-browser-react";
import type { ReactNode } from "react";
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
	Auth0Provider: ({ children }: { children: ReactNode }) => children,
}));
