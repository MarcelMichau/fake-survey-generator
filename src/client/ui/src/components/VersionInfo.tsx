import { useState, useEffect } from "react";
import type * as Types from "../types";

const VersionInfo = () => {
	const [apiVersion, setApiVersion] = useState({} as Types.ApiVersionModel);
	const [isLoading, setIsLoading] = useState(false);

	useEffect(() => {
		const getApiVersion = async () => {
			setIsLoading(true);

			const response = await fetch("api/admin/version");
			const versionResponse: Types.ApiVersionModel = await response.json();
			setIsLoading(false);

			setApiVersion(versionResponse);
		};

		getApiVersion();
	}, []);

	return (
		<>
			<span className="block text-xs font-mono font-bold uppercase">
				UI Version: {import.meta.env.VITE_APP_VERSION}
			</span>

			<span className="block text-xs font-mono font-bold uppercase">
				{isLoading ? (
					<span data-test="version-info">Loading API Version...</span>
				) : (
					<span data-test="version-info">
						API Version: {apiVersion.assemblyFileVersion}
					</span>
				)}
			</span>
		</>
	);
};
export default VersionInfo;
