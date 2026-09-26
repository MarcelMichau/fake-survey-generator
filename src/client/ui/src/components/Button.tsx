import type React from "react";

type ButtonType = "button" | "submit";
type ButtonActionType = "primary" | "secondary" | "destructive";

type ButtonProps = {
	type?: ButtonType;
	onClick?: (e: React.MouseEvent) => void;
	disabled?: boolean;
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
}: ButtonProps) => (
	<button
		type={type}
		className={`brutal-button ${actionType === "primary" ? "" : `brutal-button--${actionType}`} ${additionalClasses.join(" ")}`}
		onClick={onClick}
		disabled={disabled}
	>
		{children}
	</button>
);

export default Button;
