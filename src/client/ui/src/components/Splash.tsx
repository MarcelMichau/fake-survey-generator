import React from "react";
import { ReactComponent as Illustration } from "../assets/undraw_customer_survey_f9ur.svg";

const Splash = () => (
    <div className="container mx-auto">
        <div className="flex justify-center my-3">
            <h1 className="dark:text-white sm:text-4xl md:text-5xl lg:text-5xl xl:text-6xl font-semibold tracking-tight">
                Fake Survey Generator
            </h1>
        </div>
        <div className="flex justify-center mt-3 mb-6">
            <p className="dark:text-white text-lg">
                This is an app. That generates surveys. Fake ones. For fun. That
                is all.
            </p>
        </div>
        <div className="flex justify-center my-3">
            <Illustration />
        </div>
        <div className="flex justify-center mt-6">
            <p className="dark:text-white text-md">
                Sign In/Register to view/create surveys
            </p>
        </div>
    </div>
);

export default Splash;
