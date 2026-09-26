import { useEffect, useRef } from "react";
import Button from "./Button";

type ConfirmDialogProps = {
	open: boolean;
	title: string;
	message: string;
	confirmLabel?: string;
	cancelLabel?: string;
	busy?: boolean;
	fallbackFocus?: () => HTMLElement | null;
	onConfirm: () => void;
	onCancel: () => void;
};

const ConfirmDialog = ({
	open,
	title,
	message,
	confirmLabel = "Confirm",
	cancelLabel = "Cancel",
	busy = false,
	fallbackFocus,
	onConfirm,
	onCancel,
}: ConfirmDialogProps) => {
	const cancelRef = useRef<HTMLButtonElement>(null);
	const panelRef = useRef<HTMLDivElement>(null);
	const onCancelRef = useRef(onCancel);
	const fallbackFocusRef = useRef(fallbackFocus);
	onCancelRef.current = onCancel;
	fallbackFocusRef.current = fallbackFocus;
	useEffect(() => {
		if (!open) return;
		const previous = document.activeElement as HTMLElement | null;
		cancelRef.current?.focus();
		return () => {
			if (previous?.isConnected) previous.focus();
			else fallbackFocusRef.current?.()?.focus();
		};
	}, [open]);
	useEffect(() => {
		if (!open) return;
		if (busy) panelRef.current?.focus();
		else if (
			document.activeElement === panelRef.current ||
			!panelRef.current?.contains(document.activeElement)
		)
			cancelRef.current?.focus();
	}, [open, busy]);
	useEffect(() => {
		if (!open) return;
		const onKey = (e: KeyboardEvent) => {
			if (e.key === "Escape" && !busy) onCancelRef.current();
			if (e.key === "Tab") {
				const buttons = Array.from(
					panelRef.current?.querySelectorAll<HTMLButtonElement>(
						"button:not(:disabled)",
					) ?? [],
				);
				if (!buttons.length) {
					e.preventDefault();
					return;
				}
				const first = buttons[0];
				const last = buttons[buttons.length - 1];
				if (e.shiftKey && document.activeElement === first) {
					e.preventDefault();
					last.focus();
				} else if (!e.shiftKey && document.activeElement === last) {
					e.preventDefault();
					first.focus();
				}
			}
		};
		window.addEventListener("keydown", onKey);
		return () => window.removeEventListener("keydown", onKey);
	}, [open, busy]);

	if (!open) return null;
	return (
		<div
			className="fixed inset-0 z-50 flex items-center justify-center bg-black/85"
			role="dialog"
			aria-modal="true"
			aria-labelledby="confirm-dialog-title"
			onClick={(event) => {
				if (!busy && event.target === event.currentTarget) onCancel();
			}}
			onKeyDown={(event) => {
				if (
					!busy &&
					event.target === event.currentTarget &&
					(event.key === "Enter" || event.key === " ")
				)
					onCancel();
			}}
		>
			<div
				ref={panelRef}
				tabIndex={-1}
				className="brutal-panel mx-4 w-full max-w-md p-6"
			>
				<h3 id="confirm-dialog-title" className="display-title mb-3 text-3xl">
					{title}
				</h3>
				<p className="mb-6 text-paper">{message}</p>
				<div className="flex flex-wrap justify-end gap-4">
					<span
						ref={(element) => {
							cancelRef.current = element?.querySelector("button") ?? null;
						}}
					>
						<Button
							type="button"
							actionType="secondary"
							onClick={onCancel}
							disabled={busy}
						>
							{cancelLabel}
						</Button>
					</span>
					<Button
						type="button"
						actionType="destructive"
						onClick={onConfirm}
						disabled={busy}
					>
						{busy ? "Working..." : confirmLabel}
					</Button>
				</div>
			</div>
		</div>
	);
};

export default ConfirmDialog;
