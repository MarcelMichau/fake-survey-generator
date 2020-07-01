import React from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCopyright } from "@fortawesome/free-solid-svg-icons";

const Footer = () => (
    <footer className="flex items-center justify-center flex-wrap bg-indigo-600 p-6">
        <span className="text-white">
            Marcel Michau
            <FontAwesomeIcon icon={faCopyright} className="mx-2" />
            {new Date().getFullYear()}
        </span>
    </footer>
);

export default Footer;
