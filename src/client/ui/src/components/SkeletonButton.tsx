import type React from "react";
import Button from "./Button";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";

type ButtonType = "button" | "submit";

type ButtonActionType = "primary" | "secondary" | "destructive";

type SkeletonButtonProps = {
	loading: boolean;
	type?: ButtonType;
	onClick?: (e: React.MouseEvent) => void;
	actionType?: ButtonActionType;
	additionalClasses?: string[];
	children: React.ReactNode;
};

const SkeletonButton = ({
	loading,
	type = "button",
	onClick,
	actionType = "primary",
	additionalClasses = [],
	children,
}: SkeletonButtonProps) => {
	const styleMap = {
		primary: {
			hexValue: "#48bb78",
		},
		secondary: {
			hexValue: "#4299e1",
		},
		destructive: {
			hexValue: "#f56565",
		},
	};

	const hexColour = styleMap[actionType].hexValue;

	return (
		<SkeletonTheme baseColor="#2d3748" highlightColor={hexColour}>
			{loading ? (
				<Skeleton
					height={44}
					width={150}
					className="px-5 py-2.5 rounded-md shadow-md"
				/>
			) : (
				<Button
					type={type}
					onClick={onClick}
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
