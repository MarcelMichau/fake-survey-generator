import { useAuth0 } from "@auth0/auth0-react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { type ReactNode, useEffect, useRef, useState } from "react";

export function createQueryClient() {
	return new QueryClient({
		defaultOptions: {
			queries: { retry: false, staleTime: 30_000, refetchOnWindowFocus: false },
			mutations: { retry: false },
		},
	});
}

function QuerySession({ children }: { children: ReactNode }) {
	const [client] = useState(createQueryClient);
	const mounted = useRef(false);
	useEffect(() => {
		mounted.current = true;
		return () => {
			mounted.current = false;
			// StrictMode immediately replays effects. Only clear a discarded session.
			queueMicrotask(() => {
				if (!mounted.current) client.clear();
			});
		};
	}, [client]);
	return <QueryClientProvider client={client}>{children}</QueryClientProvider>;
}

export function ApiQueryProvider({ children }: { children: ReactNode }) {
	const { isAuthenticated, user } = useAuth0();
	return (
		<QuerySession key={isAuthenticated ? user?.sub : "anonymous"}>
			{children}
		</QuerySession>
	);
}
