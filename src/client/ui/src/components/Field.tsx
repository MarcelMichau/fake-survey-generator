import type React from "react";
import { useId } from "react";
import Skeleton from "react-loading-skeleton";

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
	const id = useId();
	return (
		<div className="mb-4">
			<label className="ui-label" htmlFor={id}>
				<span data-testid="field-label">
					{props.loading ? <Skeleton width={200} /> : props.label}
				</span>
			</label>
			{props.loading ? (
				<Skeleton height={44} />
			) : (
				<div className="flex flex-wrap items-center gap-3">
					<input
						id={id}
						className="brutal-input min-w-0 flex-1"
						type="text"
						value={props.value}
						placeholder={props.placeholder}
						onChange={(e) => props.onChange(e.target.value)}
					/>
					{props.children}
				</div>
			)}
		</div>
	);
}

export default Field;
