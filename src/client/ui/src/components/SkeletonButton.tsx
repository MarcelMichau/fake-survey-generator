import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import Button, { type ButtonProps } from "./Button";

type SkeletonButtonProps = ButtonProps & {
	loading: boolean;
};

const SkeletonButton = ({
	loading,
	type = "button",
	onClick,
	disabled = false,
	actionType = "primary",
	additionalClasses = [],
	children,
	...buttonProps
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
					{...buttonProps}
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
