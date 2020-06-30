import React from "react";

type AlertType = "error" | "success";

type AlertProps = {
    title: string;
    message: string;
    type?: AlertType;
};

const Alert: React.FC<AlertProps> = ({ title, message, type = "success" }) => {
    const colour = type === "success" ? "green" : "red";

    return (
        <div
            className={`bg-${colour}-800 border border-${colour}-500 text-gray-200 px-4 py-3 my-4 rounded relative`}
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
