import React from "react";

const VersionInfo = () => (
    <small>UI Version: {process.env.REACT_APP_VERSION}</small>
);
export default VersionInfo;
