import { useQuery } from "@tanstack/react-query";
import { ApiError } from "../api/ApiError";
import { publicClient } from "../api/publicClient";

const VersionInfo = () => {
	const version = useQuery({
		queryKey: ["api-version"],
		staleTime: Infinity,
		queryFn: async ({ signal }) => {
			const { data, response } = await publicClient.GET("/api/admin/version", {
				signal,
			});
			if (!response.ok || !data)
				throw new ApiError("API version unavailable", response.status);
			return data;
		},
	});

	return (
		<>
			<span className="block text-xs font-mono font-bold uppercase">
				UI Version: {import.meta.env.VITE_APP_VERSION}
			</span>

			<span className="block text-xs font-mono font-bold uppercase">
				{version.isPending ? (
					<span data-test="version-info">Loading API Version...</span>
				) : (
					<span data-test="version-info">
						API Version: {version.data?.assemblyFileVersion ?? "unavailable"}
					</span>
				)}
			</span>
		</>
	);
};
export default VersionInfo;
