import React from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";

type ButtonType = "button" | "submit";

type ButtonActionType = "primary" | "secondary" | "destructive";

type ButtonProps = {
    children: React.ReactNode;
    onClick?: (e: React.MouseEvent) => void;
    type?: ButtonType;
    loading: boolean;
    actionType?: ButtonActionType;
};

const Button: React.FC<ButtonProps> = ({
    children,
    onClick,
    type = "button",
    loading,
    actionType = "primary",
}) => {
    const colourMap = {
        primary: { value: "green", hexValue: "#48bb78" },
        secondary: { value: "blue", hexValue: "#4299e1" },
        destructive: { value: "red", hexValue: "#f56565" },
    };

    const friendlyColour = colourMap[actionType].value;
    const hexColour = colourMap[actionType].hexValue;

    return (
        <SkeletonTheme color="#2d3748" highlightColor={hexColour}>
            {loading ? (
                <Skeleton height={40} width={150} className="px-4 py-2" />
            ) : (
                <button
                    type={type}
                    className={`align-baseline px-4 py-2 border rounded text-white bg-${friendlyColour}-500 border-${friendlyColour}-500 hover:text-white hover:bg-${friendlyColour}-400`}
                    onClick={onClick}
                >
                    {children}
                </button>
            )}
        </SkeletonTheme>
    );
};
export default Button;
