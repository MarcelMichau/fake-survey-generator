import React from "react";
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

const SkeletonButton: React.FC<SkeletonButtonProps> = ({
    loading,
    type = "button",
    onClick,
    actionType = "primary",
    additionalClasses = [],
    children,
}) => {
    const colourMap = {
        primary: {
            classes: "bg-green-500 hover:bg-green-400 active:bg-green-600",
            hexValue: "#48bb78",
        },
        secondary: {
            classes: "bg-blue-500 hover:bg-blue-400 active:bg-blue-600",
            hexValue: "#4299e1",
        },
        destructive: {
            classes: "bg-red-500 hover:bg-red-400 active:bg-red-600",
            hexValue: "#f56565",
        },
    };

    const hexColour = colourMap[actionType].hexValue;

    return (
        <SkeletonTheme baseColor="#2d3748" highlightColor={hexColour}>
            {loading ? (
                <Skeleton height={40} width={150} className="px-4 py-2" />
            ) : (
                <Button
                    type={type}
                    onClick={onClick}
                    actionType={actionType}
                    additionalClasses={additionalClasses}
                    children={children}
                ></Button>
            )}
        </SkeletonTheme>
    );
};
export default SkeletonButton;
