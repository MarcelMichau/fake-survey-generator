export class ApiError extends Error {
	readonly status: number;
	readonly validationErrors: string[];
	constructor(
		message: string,
		status: number,
		validationErrors: string[] = [],
	) {
		super(message);
		this.status = status;
		this.validationErrors = validationErrors;
	}
}

export function validationMessages(error: unknown): string[] {
	if (!error || typeof error !== "object") return [];
	return Object.values(error).flatMap((value) =>
		Array.isArray(value)
			? value.filter((item): item is string => typeof item === "string")
			: [],
	);
}
