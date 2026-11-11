import type React from "react";

type ButtonType = "button" | "submit";
type ButtonActionType = "primary" | "secondary" | "destructive";

export type ButtonProps = Omit<
	React.ComponentPropsWithRef<"button">,
	"type" | "className"
> & {
	type?: ButtonType;
	actionType?: ButtonActionType;
	additionalClasses?: string[];
	children: React.ReactNode;
};

const Button = ({
	type = "button",
	onClick,
	disabled = false,
	actionType = "primary",
	additionalClasses = [],
	children,
	...buttonProps
}: ButtonProps) => (
	<button
		{...buttonProps}
		type={type}
		className={`brutal-button ${actionType === "primary" ? "" : `brutal-button--${actionType}`} ${additionalClasses.join(" ")}`}
		onClick={onClick}
		disabled={disabled}
	>
		{children}
	</button>
);

export default Button;
