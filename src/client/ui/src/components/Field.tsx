import React from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";

type FieldValue = string | number;

type FieldProps<T extends FieldValue> = {
    label: string;
    value: T;
    placeholder?: string;
    onChange: (value: string) => void;
    loading: boolean;
    children?: React.ReactNode;
};

function Field<T extends FieldValue>(props: FieldProps<T>) {
    return (
        <SkeletonTheme color="#2d3748" highlightColor="#4a5568">
            <label className="block text-gray-400">
                {props.loading ? <Skeleton width={250} /> : props.label}
                <div>
                    {props.loading ? (
                        <Skeleton height={38} className="py-2 mt-1 mb-3" />
                    ) : (
                        <>
                            <input
                                className="appearance-none border border-gray-700 rounded w-full lg:w-1/2 py-2 px-3 text-gray-200 bg-gray-700 leading-tight focus:outline-none focus:ring mt-1 mb-3"
                                type="text"
                                value={props.value}
                                placeholder={props.placeholder}
                                onChange={(e) => props.onChange(e.target.value)}
                            />
                            {props.children}
                        </>
                    )}
                </div>
            </label>
        </SkeletonTheme>
    );
}

export default Field;
