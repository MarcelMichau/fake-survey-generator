import type React from "react";

type ButtonType = "button" | "submit";

type ButtonActionType = "primary" | "secondary" | "destructive";

type ButtonProps = {
	type?: ButtonType;
	onClick?: (e: React.MouseEvent) => void;
	actionType?: ButtonActionType;
	additionalClasses?: string[];
	children: React.ReactNode;
};

const Button = ({
	type = "button",
	onClick,
	actionType = "primary",
	additionalClasses = [],
	children,
}: ButtonProps) => {
	const styleMap = {
		primary: {
			classes:
				"bg-gradient-to-r from-green-500 to-emerald-600 hover:from-green-400 hover:to-emerald-500 active:from-green-600 active:to-emerald-700",
			hexValue: "#48bb78",
		},
		secondary: {
			classes:
				"bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-400 hover:to-indigo-500 active:from-blue-600 active:to-indigo-700",
			hexValue: "#4299e1",
		},
		destructive: {
			classes:
				"bg-gradient-to-r from-red-500 to-rose-600 hover:from-red-400 hover:to-rose-500 active:from-red-600 active:to-rose-700",
			hexValue: "#f56565",
		},
	};

	const classes = styleMap[actionType].classes;

	return (
		<button
			type={type}
			className={`align-baseline px-5 py-2.5 rounded-md text-white font-medium ${classes} focus:ring-2 focus:ring-offset-2 focus:ring-offset-gray-900 focus:ring-opacity-60 focus:outline-none shadow-md transition-all duration-200 ease-in-out transform hover:scale-[1.02] ${additionalClasses.join(
				" ",
			)}`}
			onClick={onClick}
		>
			{children}
		</button>
	);
};
export default Button;
