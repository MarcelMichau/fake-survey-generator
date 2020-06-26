import React, { useState, useEffect } from "react";
import * as Types from "../types";

const VersionInfo = () => {
    const [apiVersion, setApiVersion] = useState({} as Types.ApiVersionModel);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        const getApiVersion = async () => {
            setIsLoading(true);
            const response = await fetch("api/admin/version");
            const versionResponse: Types.ApiVersionResponse = await response.json();
            setIsLoading(false);

            setApiVersion(versionResponse.result);
        };

        getApiVersion();
    }, []);

    return (
        <>
            <div>
                <small>UI Version: {process.env.REACT_APP_VERSION}</small>
            </div>

            {isLoading ? (
                <div>
                    <small>Loading API Version...</small>{" "}
                </div>
            ) : (
                <div>
                    <small>API Version: {apiVersion.assemblyFileVersion}</small>
                </div>
            )}
        </>
    );
};
export default VersionInfo;
