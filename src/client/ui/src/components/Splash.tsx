import React from "react";
import { ReactComponent as Illustration } from "../assets/undraw_customer_survey_f9ur.svg";

const Splash = () => (
    <div className="container mx-auto px-5">
        <div className="text-center my-3">
            <h1 className="dark:text-white text-4xl md:text-5xl lg:text-5xl xl:text-6xl font-semibold tracking-tight">
                Fake Survey Generator
            </h1>
        </div>
        <div className="text-center sm:mb-2 mt-3 mb-6">
            <p className="dark:text-white text-lg">
                This is an app. That generates surveys. Fake ones. For fun. That
                is all.
            </p>
        </div>
        <div className="flex justify-center my-3 md:my-6 lg:my-10 xl:my-12">
            <Illustration className="h-64"></Illustration>
        </div>
        <div className="flex justify-center mt-6">
            <p className="dark:text-white text-md">
                Sign In/Register to view/create surveys
            </p>
        </div>
    </div>
);

export default Splash;
