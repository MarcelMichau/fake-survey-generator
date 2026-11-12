import { useEffect, useId, useLayoutEffect, useRef } from "react";
import Alert from "./Alert";
import Button from "./Button";

type ConfirmDialogProps = {
	open: boolean;
	title: string;
	message: string;
	confirmLabel?: string;
	cancelLabel?: string;
	busy?: boolean;
	error?: string;
	invoker?: HTMLElement | null;
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
	error,
	invoker,
	fallbackFocus,
	onConfirm,
	onCancel,
}: ConfirmDialogProps) => {
	const id = useId();
	const dialogRef = useRef<HTMLDialogElement>(null);
	const cancelRef = useRef<HTMLButtonElement>(null);
	const panelRef = useRef<HTMLDivElement>(null);
	const pressedOutside = useRef(false);
	const latest = useRef({ open, busy, onCancel, fallbackFocus });
	useLayoutEffect(() => {
		latest.current = { open, busy, onCancel, fallbackFocus };
	});

	useEffect(() => {
		const dialog = dialogRef.current;
		if (!open || !dialog) return;
		const previous = invoker ?? document.activeElement;
		// Pointer activation need not focus a button in every browser. Give the native
		// dialog the actual invoker so its built-in focus return still works there.
		if (invoker?.isConnected) invoker.focus();
		dialog.showModal();
		cancelRef.current?.focus();
		return () => {
			dialog.close();
			// The browser restores the invoker unless a successful deletion removed it.
			queueMicrotask(() => {
				if (!previous?.isConnected) latest.current.fallbackFocus?.()?.focus();
			});
		};
	}, [open, invoker]);

	useEffect(() => {
		if (!open) return;
		if (busy) panelRef.current?.focus();
		else cancelRef.current?.focus();
	}, [open, busy]);

	const outside = (event: React.PointerEvent<HTMLDialogElement>) => {
		const rect = event.currentTarget.getBoundingClientRect();
		return (
			event.clientX < rect.left ||
			event.clientX > rect.right ||
			event.clientY < rect.top ||
			event.clientY > rect.bottom
		);
	};

	return (
		<dialog
			ref={dialogRef}
			className="confirmation-dialog"
			aria-labelledby={`${id}-title`}
			aria-describedby={`${id}-message`}
			aria-busy={busy}
			closedby={busy ? "none" : "any"}
			onCancel={(event) => {
				if (busy) event.preventDefault();
			}}
			onClose={() => {
				const state = latest.current;
				if (!state.open || dialogRef.current?.open) return;
				// Older implementations can still dismiss on repeated Escape while busy.
				if (state.busy) {
					dialogRef.current?.showModal();
					panelRef.current?.focus();
				} else state.onCancel();
			}}
			onPointerDown={(event) => {
				pressedOutside.current = outside(event);
			}}
			onPointerCancel={() => {
				pressedOutside.current = false;
			}}
			onPointerUp={(event) => {
				if (
					!("closedBy" in event.currentTarget) &&
					!busy &&
					pressedOutside.current &&
					outside(event)
				)
					onCancel();
				pressedOutside.current = false;
			}}
		>
			<div ref={panelRef} tabIndex={-1} className="brutal-panel p-6">
				<h2 id={`${id}-title`} className="display-title mb-3 text-3xl">
					{title}
				</h2>
				<p id={`${id}-message`} className="mb-6 text-paper wrap-break-word">
					{message}
				</p>
				{error && (
					<Alert type="error" title="Deletion failed" message={error} />
				)}
				<div className="flex flex-wrap justify-end gap-4">
					<Button
						ref={cancelRef}
						autoFocus
						actionType="secondary"
						onClick={onCancel}
						disabled={busy}
					>
						{cancelLabel}
					</Button>
					<Button actionType="destructive" onClick={onConfirm} disabled={busy}>
						{busy ? "Working..." : confirmLabel}
					</Button>
				</div>
			</div>
		</dialog>
	);
};

export default ConfirmDialog;
