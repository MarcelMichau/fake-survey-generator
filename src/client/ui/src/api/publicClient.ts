import createClient from "openapi-fetch";
import type { paths } from "./generated";

export const publicClient = createClient<paths>({
	baseUrl: window.location.origin,
});
