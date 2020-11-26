import React from "react";

type ButtonType = "button" | "submit";

type ButtonActionType = "primary" | "secondary" | "destructive";

type ButtonProps = {
    type?: ButtonType;
    onClick?: (e: React.MouseEvent) => void;
    actionType?: ButtonActionType;
    additionalClasses?: string[];
    children: React.ReactNode;
};

const Button: React.FC<ButtonProps> = ({
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

    const classes = colourMap[actionType].classes;

    return (
        <button
            type={type}
            className={`align-baseline px-4 py-2 rounded text-white ${classes} focus:ring ${additionalClasses.join(
                " "
            )}`}
            onClick={onClick}
        >
            {children}
        </button>
    );
};
export default Button;
