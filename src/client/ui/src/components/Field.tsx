import React from "react";

type FieldValue = string | number;

type FieldProps<T extends FieldValue> = {
    label: string;
    value: T;
    onChange: (value: string) => void;
};

function Field<T extends FieldValue>(props: FieldProps<T>) {
    return (
        <label className="block text-gray-500 text-sm">
            {props.label}
            <div>
                <input
                    className="appearance-none border border-gray-700 rounded w-1/4 py-2 px-3 text-gray-200 bg-gray-800 leading-tight focus:outline-none focus:shadow-outline my-2"
                    type="text"
                    value={props.value}
                    onChange={(e) => props.onChange(e.target.value)}
                />
            </div>
        </label>
    );
}

export default Field;
