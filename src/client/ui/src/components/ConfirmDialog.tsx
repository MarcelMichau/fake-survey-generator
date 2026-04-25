import { useEffect } from "react";
import Button from "./Button";

type ConfirmDialogProps = {
	open: boolean;
	title: string;
	message: string;
	confirmLabel?: string;
	cancelLabel?: string;
	busy?: boolean;
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
	onConfirm,
	onCancel,
}: ConfirmDialogProps) => {
	useEffect(() => {
		if (!open) return;
		const onKey = (e: KeyboardEvent) => {
			if (e.key === "Escape" && !busy) onCancel();
		};
		window.addEventListener("keydown", onKey);
		return () => window.removeEventListener("keydown", onKey);
	}, [open, busy, onCancel]);

	if (!open) return null;

	return (
		<div
			className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm animate-fade-in"
			role="dialog"
			aria-modal="true"
			aria-labelledby="confirm-dialog-title"
			onClick={() => {
				if (!busy) onCancel();
			}}
			onKeyDown={() => {}}
		>
			<div
				className="dark:bg-gray-800 border border-gray-700 rounded-lg shadow-xl max-w-md w-full mx-4 p-6"
				onClick={(e) => e.stopPropagation()}
				onKeyDown={() => {}}
			>
				<h3
					id="confirm-dialog-title"
					className="dark:text-indigo-400 text-xl font-semibold tracking-tight mb-3"
				>
					{title}
				</h3>
				<p className="text-gray-300 mb-6">{message}</p>
				<div className="flex justify-end gap-3">
					<Button
						type="button"
						actionType="secondary"
						onClick={onCancel}
						additionalClasses={busy ? ["opacity-60", "cursor-not-allowed"] : []}
					>
						{cancelLabel}
					</Button>
					<Button
						type="button"
						actionType="destructive"
						onClick={onConfirm}
						additionalClasses={busy ? ["opacity-60", "cursor-not-allowed"] : []}
					>
						{busy ? "Working..." : confirmLabel}
					</Button>
				</div>
			</div>
		</div>
	);
};

export default ConfirmDialog;
