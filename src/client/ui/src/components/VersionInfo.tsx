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
			<span className="block mt-4 lg:inline-block lg:mt-0 text-white mr-4">
				UI Version: {import.meta.env.VITE_APP_VERSION}
			</span>

			<span className="block mt-4 lg:inline-block lg:mt-0 text-white">
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
