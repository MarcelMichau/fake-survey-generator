import type React from "react";
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
		<SkeletonTheme baseColor="#2d3748" highlightColor="#4a5568">
			<label
				className="block text-gray-300 font-medium mb-1"
				htmlFor="field-input"
			>
				{props.loading ? <Skeleton width={250} /> : props.label}
				<div>
					{props.loading ? (
						<Skeleton height={42} className="py-2 mt-1 mb-3" />
					) : (
						<>
							<input
								id="field-input"
								className="appearance-none border border-gray-600 rounded-md w-full lg:w-2/3 py-2.5 px-4 text-gray-100 bg-gray-700/70 leading-tight focus:outline-none focus:ring-2 focus:ring-indigo-500/50 focus:border-indigo-400 transition-all duration-200 mt-1 mb-3 placeholder:text-gray-400 shadow-sm backdrop-blur-sm"
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
