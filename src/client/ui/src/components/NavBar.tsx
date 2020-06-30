import React, { useState } from "react";
import AuthButton from "./AuthButton";
import VersionInfo from "./VersionInfo";

const NavBar = () => {
    const [collapsed, setCollapsed] = useState(true);

    return (
        <nav className="flex items-center justify-between flex-wrap bg-teal-500 p-6">
            <div className="flex items-center flex-shrink-0 text-white mr-6">
                <span className="font-semibold text-xl tracking-tight">
                    Fake Survey Generator
                </span>
            </div>
            <div className="block lg:hidden">
                <button
                    className="flex items-center px-3 py-2 border rounded text-teal-200 border-teal-400 hover:text-white hover:border-white"
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
                        className="block mt-4 lg:inline-block lg:mt-0 text-teal-200 hover:text-white mr-4"
                    >
                        API Swagger
                    </a>
                    <a
                        href="/health/ready"
                        className="block mt-4 lg:inline-block lg:mt-0 text-teal-200 hover:text-white mr-4"
                    >
                        Health Check
                    </a>
                    <a
                        href="https://github.com/MarcelMichau/fake-survey-generator"
                        className="block mt-4 lg:inline-block lg:mt-0 text-teal-200 hover:text-white mr-4"
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
