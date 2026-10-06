import { useAuth0 } from "@auth0/auth0-react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { type ReactNode, useEffect, useRef, useState } from "react";
import { SurveySession } from "./SurveySession";

export function createQueryClient() {
	return new QueryClient({
		defaultOptions: {
			queries: { retry: false, staleTime: 30_000, refetchOnWindowFocus: false },
			mutations: { retry: false },
		},
	});
}

function QuerySession({
	children,
	userId,
}: {
	children: ReactNode;
	userId: string | undefined;
}) {
	const [client] = useState(createQueryClient);
	const [cancellation] = useState(() => new AbortController());
	const mounted = useRef(false);
	useEffect(() => {
		mounted.current = true;
		return () => {
			mounted.current = false;
			// StrictMode immediately replays effects. Only clear a discarded session.
			queueMicrotask(() => {
				if (!mounted.current) {
					cancellation.abort();
					client.clear();
				}
			});
		};
	}, [client, cancellation]);
	return (
		<QueryClientProvider client={client}>
			<SurveySession userId={userId} signal={cancellation.signal}>
				{children}
			</SurveySession>
		</QueryClientProvider>
	);
}

export function ApiQueryProvider({ children }: { children: ReactNode }) {
	const { isAuthenticated, user } = useAuth0();
	return (
		<QuerySession
			key={isAuthenticated ? user?.sub : "anonymous"}
			userId={isAuthenticated ? user?.sub : undefined}
		>
			{children}
		</QuerySession>
	);
}
