import type React from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import Button from "./Button";

type ButtonType = "button" | "submit";

type ButtonActionType = "primary" | "secondary" | "destructive";

type SkeletonButtonProps = {
	loading: boolean;
	type?: ButtonType;
	onClick?: (e: React.MouseEvent) => void;
	disabled?: boolean;
	actionType?: ButtonActionType;
	additionalClasses?: string[];
	children: React.ReactNode;
};

const SkeletonButton = ({
	loading,
	type = "button",
	onClick,
	disabled = false,
	actionType = "primary",
	additionalClasses = [],
	children,
}: SkeletonButtonProps) => {
	const styleMap = {
		primary: {
			hexValue: "#c7ff18",
		},
		secondary: {
			hexValue: "#f5f6f3",
		},
		destructive: {
			hexValue: "#ff8e83",
		},
	};

	const hexColour = styleMap[actionType].hexValue;

	return (
		<SkeletonTheme baseColor="#30353a" highlightColor={hexColour}>
			{loading ? (
				<Skeleton height={44} width={150} className="px-5 py-2.5" />
			) : (
				<Button
					type={type}
					onClick={onClick}
					disabled={disabled}
					actionType={actionType}
					additionalClasses={additionalClasses}
				>
					{children}
				</Button>
			)}
		</SkeletonTheme>
	);
};
export default SkeletonButton;
