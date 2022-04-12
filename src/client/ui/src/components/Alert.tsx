import React from "react";

type AlertType = "error" | "success";

type AlertProps = {
    title: string;
    message: string;
    type?: AlertType;
};

const Alert = ({ title, message, type = "success" }: AlertProps) => {
    const classes =
        type === "success"
            ? "bg-green-800 border-green-500"
            : "bg-red-800 border-red-500";

    return (
        <div
            className={`${classes} border text-gray-200 px-4 py-3 my-4 rounded relative`}
            role="alert"
        >
            <div>
                <strong className="font-bold">{title}</strong>
            </div>
            <div>
                <span className="block sm:inline">{message}</span>
            </div>
        </div>
    );
};

export default Alert;
