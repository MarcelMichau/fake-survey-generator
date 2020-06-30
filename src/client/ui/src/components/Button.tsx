import React from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";

type ButtonType = "button" | "submit";

type ButtonProps = {
    text: string;
    onClick?: (e: React.MouseEvent) => void;
    type?: ButtonType;
    loading: boolean;
};

const Button: React.FC<ButtonProps> = ({
    text,
    onClick,
    type = "button",
    loading,
}) => (
    <SkeletonTheme color="#2d3748" highlightColor="#48bb78">
        {loading ? (
            <Skeleton width={150} className="px-4 py-2" />
        ) : (
            <button
                type={type}
                className="align-baseline px-4 py-2 border rounded text-white bg-green-500 border-green-500 hover:text-white hover:bg-green-400"
                onClick={onClick}
            >
                {text}
            </button>
        )}
    </SkeletonTheme>
);

export default Button;
