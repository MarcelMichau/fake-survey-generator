import React from "react";

type ButtonType = "button" | "submit";

type ButtonProps = {
    text: string;
    onClick?: (e: React.MouseEvent) => void;
    type?: ButtonType;
};

const Button: React.FC<ButtonProps> = ({ text, onClick, type = "button" }) => (
    <button
        type={type}
        className="align-baseline px-4 py-2 border rounded text-white bg-green-500 border-green-500 hover:text-white hover:bg-green-400"
        onClick={onClick}
    >
        {text}
    </button>
);

export default Button;
