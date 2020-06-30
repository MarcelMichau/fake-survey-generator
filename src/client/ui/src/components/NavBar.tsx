import React, { useState } from "react";
import AuthButton from "./AuthButton";
import VersionInfo from "./VersionInfo";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPollH } from "@fortawesome/free-solid-svg-icons";

const NavBar = () => {
    const [collapsed, setCollapsed] = useState(true);

    return (
        <nav className="flex items-center justify-between flex-wrap bg-indigo-600 p-6">
            <div className="flex items-center flex-shrink-0 text-white mr-6">
                <span className="font-semibold text-xl tracking-tight">
                    Fake Survey Generator
                    <FontAwesomeIcon icon={faPollH} className="ml-2" />
                </span>
            </div>
            <div className="block lg:hidden">
                <button
                    className="flex items-center px-3 py-2 border rounded text-indigo-200 border-indigo-400 hover:text-white hover:border-white"
                    onClick={() => setCollapsed(!collapsed)}
                >
                    <svg
                        className="fill-current h-3 w-3"
                        viewBox="0 0 20 20"
                        xmlns="http://www.w3.org/2000/svg"
                    >
                        <title>Menu</title>
                        <path d="M0 3h20v2H0V3zm0 6h20v2H0V9zm0 6h20v2H0v-2z" />
                    </svg>
                </button>
            </div>
            <div
                className={`w-full ${
                    collapsed ? "hidden" : "block"
                } flex-grow lg:flex lg:items-center lg:w-auto`}
            >
                <div className="text-sm lg:flex-grow">
                    <a
                        href="/swagger"
                        className="block mt-4 lg:inline-block lg:mt-0 text-indigo-200 hover:text-white mr-4"
                    >
                        API Swagger
                    </a>
                    <a
                        href="/health/ready"
                        className="block mt-4 lg:inline-block lg:mt-0 text-indigo-200 hover:text-white mr-4"
                    >
                        Health Check
                    </a>
                    <a
                        href="https://github.com/MarcelMichau/fake-survey-generator"
                        className="block mt-4 lg:inline-block lg:mt-0 text-indigo-200 hover:text-white mr-4"
                    >
                        GitHub Repo
                    </a>
                    <VersionInfo></VersionInfo>
                </div>
                <div>
                    <AuthButton></AuthButton>
                </div>
            </div>
        </nav>
    );
};

export default NavBar;
