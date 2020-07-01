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
        primary: {
            classes: "bg-green-500 border-green-500 hover:bg-green-400",
            hexValue: "#48bb78",
        },
        secondary: {
            classes: "bg-blue-500 border-blue-500 hover:bg-blue-400",
            hexValue: "#4299e1",
        },
        destructive: {
            classes: "bg-red-500 border-red-500 hover:bg-red-400",
            hexValue: "#f56565",
        },
    };

    const classes = colourMap[actionType].classes;
    const hexColour = colourMap[actionType].hexValue;

    return (
        <SkeletonTheme color="#2d3748" highlightColor={hexColour}>
            {loading ? (
                <Skeleton height={40} width={150} className="px-4 py-2" />
            ) : (
                <button
                    type={type}
                    className={`align-baseline px-4 py-2 border rounded text-white ${classes} hover:text-white`}
                    onClick={onClick}
                >
                    {children}
                </button>
            )}
        </SkeletonTheme>
    );
};
export default Button;
