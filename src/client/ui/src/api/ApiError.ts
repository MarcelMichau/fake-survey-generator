export type FieldErrors = Record<string, string[]>;

export class ApiError extends Error {
	readonly status: number;
	readonly validationErrors: string[];
	readonly fieldErrors: FieldErrors;
	constructor(
		message: string,
		status: number,
		validationErrors: string[] | FieldErrors = [],
	) {
		super(message);
		this.status = status;
		this.fieldErrors = Array.isArray(validationErrors) ? {} : validationErrors;
		this.validationErrors = Array.isArray(validationErrors)
			? validationErrors
			: Object.values(validationErrors).flat();
	}
}

export function validationFields(error: unknown): FieldErrors {
	if (!error || typeof error !== "object") return {};
	return Object.fromEntries(
		Object.entries(error).flatMap(([field, value]) => {
			const messages = Array.isArray(value)
				? value.filter((item): item is string => typeof item === "string")
				: [];
			return messages.length ? [[field, messages]] : [];
		}),
	);
}
