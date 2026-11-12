import type React from "react";
import { useId, useLayoutEffect, useRef, useState } from "react";
import Skeleton from "react-loading-skeleton";
import { useAnnounce } from "./Announcements";

type FieldProps = Omit<
	React.ComponentPropsWithRef<"input">,
	"value" | "onChange" | "className" | "children"
> & {
	label: string;
	value: string | number;
	onChange: (value: string) => void;
	loading: boolean;
	className?: string;
	children?: React.ReactNode;
	hint?: string;
	error?: string;
};

function Field({
	label,
	value,
	onChange,
	loading,
	className,
	children,
	hint,
	error,
	id: suppliedId,
	ref: suppliedRef,
	type = "text",
	onBlur,
	onInvalid,
	"aria-describedby": describedBy,
	...inputProps
}: FieldProps) {
	const generatedId = useId();
	const id = suppliedId ?? generatedId;
	const inputRef = useRef<HTMLInputElement>(null);
	const [nativeMessage, setNativeMessage] = useState("");
	const [showNativeError, setShowNativeError] = useState(false);
	const announce = useAnnounce();
	const message = error || (showNativeError ? nativeMessage : "");
	const constraints = [
		inputProps.required,
		inputProps.min,
		inputProps.max,
		inputProps.step,
		inputProps.pattern,
		inputProps.maxLength,
	].join(":");
	// biome-ignore lint/correctness/useExhaustiveDependencies: native validity reflects controlled values and constraint attributes after the DOM commit.
	useLayoutEffect(() => {
		const input = inputRef.current;
		const text = input?.validationMessage ?? "";
		setNativeMessage(text);
		setShowNativeError(
			(shown) =>
				Boolean(text) && (shown || Boolean(input?.matches(":user-invalid"))),
		);
	}, [value, constraints, loading]);
	const descriptions =
		[describedBy, hint && `${id}-hint`, message && `${id}-error`]
			.filter(Boolean)
			.join(" ") || undefined;

	return (
		<div className={className ?? "mb-5"}>
			<label className="ui-label" htmlFor={id}>
				<span data-testid="field-label">
					{loading ? <Skeleton width={200} /> : label}
				</span>
				{!loading && inputProps.required && (
					<span aria-hidden="true"> (required)</span>
				)}
			</label>
			{hint && (
				<p id={`${id}-hint`} className="ui-hint mb-2">
					{hint}
				</p>
			)}
			{loading ? (
				<Skeleton height={44} />
			) : (
				<>
					<div className="flex flex-wrap items-center gap-3">
						<input
							{...inputProps}
							ref={(input) => {
								inputRef.current = input;
								if (typeof suppliedRef === "function")
									return suppliedRef(input);
								if (suppliedRef) suppliedRef.current = input;
							}}
							id={id}
							className="brutal-input min-w-0 flex-1"
							type={type}
							value={value}
							aria-invalid={Boolean(message)}
							aria-describedby={descriptions}
							onInvalid={(event) => {
								setNativeMessage(event.currentTarget.validationMessage);
								setShowNativeError(true);
								onInvalid?.(event);
							}}
							onBlur={(event) => {
								const input = event.currentTarget;
								if (input.matches(":user-invalid")) {
									setShowNativeError(true);
									if (!message)
										announce(`${label}: ${input.validationMessage}`, input);
								}
								onBlur?.(event);
							}}
							onChange={(event) => {
								const input = event.currentTarget;
								setNativeMessage(input.validationMessage);
								if (input.validity.valid) {
									setShowNativeError(false);
									if (message) announce("");
								} else if (input.matches(":user-invalid"))
									setShowNativeError(true);
								onChange(input.value);
							}}
						/>
						{children}
					</div>
					{/* Reserve the current message's space before blur, so revealing it cannot move a clicked button. */}
					<p
						id={`${id}-error`}
						className="field-error"
						style={{ visibility: message ? "visible" : "hidden" }}
						aria-hidden={!message}
					>
						Error: {message || nativeMessage || "\u00a0"}
					</p>
				</>
			)}
		</div>
	);
}

export default Field;
