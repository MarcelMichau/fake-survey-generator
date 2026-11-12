import {
	createContext,
	type ReactNode,
	useCallback,
	useContext,
	useEffect,
	useRef,
	useState,
} from "react";

const Announcements = createContext<
	(message: string, source?: HTMLInputElement) => void
>(() => {});

/** One polite region for field errors that appear while focus moves elsewhere. */
export function AnnouncementProvider({ children }: { children: ReactNode }) {
	const [message, setMessage] = useState("");
	const timer = useRef<ReturnType<typeof setTimeout> | undefined>(undefined);
	const announce = useCallback((text: string, source?: HTMLInputElement) => {
		clearTimeout(timer.current);
		setMessage("");
		if (text)
			timer.current = setTimeout(() => {
				if (document.querySelector("dialog:modal")) return;
				if (
					source &&
					(!source.isConnected ||
						source.matches(":disabled") ||
						source.validity.valid)
				)
					return;
				setMessage(text);
			}, 500);
	}, []);
	useEffect(() => () => clearTimeout(timer.current), []);
	return (
		<Announcements value={announce}>
			{children}
			<div role="status" className="sr-only" aria-atomic="true">
				{message}
			</div>
		</Announcements>
	);
}

export const useAnnounce = () => useContext(Announcements);
