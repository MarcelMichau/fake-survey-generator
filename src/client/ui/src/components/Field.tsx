import React from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";

type FieldValue = string | number;

type FieldProps<T extends FieldValue> = {
    label: string;
    value: T;
    placeholder?: string;
    onChange: (value: string) => void;
    loading: boolean;
};

function Field<T extends FieldValue>(props: FieldProps<T>) {
    return (
        <SkeletonTheme color="#2d3748" highlightColor="#a0aec0">
            <label className="block text-gray-400">
                {props.loading ? (
                    <Skeleton width={250} className="block" />
                ) : (
                    props.label
                )}
                <div>
                    {props.loading ? (
                        <Skeleton width={500} className="w-1/4 py-2 my-2" />
                    ) : (
                        <input
                            className="appearance-none border border-gray-700 rounded w-full md:w-1/2 py-2 px-3 text-gray-200 bg-gray-700 leading-tight focus:outline-none focus:shadow-outline my-2"
                            type="text"
                            value={props.value}
                            placeholder={props.placeholder}
                            onChange={(e) => props.onChange(e.target.value)}
                        />
                    )}
                </div>
            </label>
        </SkeletonTheme>
    );
}

export default Field;
