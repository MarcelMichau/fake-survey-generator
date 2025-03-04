import type React from "react";
import { useState, useEffect } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import NavBar from "./components/NavBar";
import CreateSurvey from "./components/CreateSurvey";
import GetSurvey from "./components/GetSurvey";
import MySurveys from "./components/MySurveys";
import Splash from "./components/Splash";
import Alert from "./components/Alert";
import Footer from "./components/Footer";
import type * as Types from "./types";

const App = (): React.JSX.Element => {
	const [errorMessage, setErrorMessage] = useState("");
	const { getAccessTokenSilently, user, isAuthenticated, isLoading } =
		useAuth0();

	useEffect(() => {
		const registerUser = async () => {
			const token = await getAccessTokenSilently();

			const response = await fetch("api/user/register", {
				method: "POST",
				headers: {
					"Content-Type": "application/json",
					Authorization: `Bearer ${token}`,
				},
			});

			try {
				await response.json();
			} catch (error) {
				setErrorMessage("Oops, something went wrong with registering a user.");
				return;
			}
		};

		const isUserRegistered = async (): Promise<boolean> => {
			const token = await getAccessTokenSilently();

			const response = await fetch(
				`api/user/isRegistered?userId=${user?.sub}`,
				{
					headers: {
						Authorization: `Bearer ${token}`,
					},
				},
			);

			try {
				const data: Types.UserRegistrationStatusModel = await response.json();
				return data.isUserRegistered;
			} catch (error) {
				setErrorMessage(
					"Oops, something went wrong getting the user registration status.",
				);
				return false;
			}
		};

		const register = async () => {
			if (isAuthenticated && user) {
				const isUserAlreadyRegistered = await isUserRegistered();

				if (!isUserAlreadyRegistered) {
					await registerUser();
				}
			}
		};

		register();
	}, [isAuthenticated, user, getAccessTokenSilently]);

	return (
		<div className="flex flex-col h-full">
			<div className="flex-1">
				<NavBar />
				{!isAuthenticated && !isLoading ? (
					<Splash />
				) : (
					<div className="grid sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-2 xl:grid-cols-2 gap-4 mb-4">
						<div>
							<CreateSurvey loading={isLoading} />
						</div>
						<div>
							<GetSurvey loading={isLoading} />
							{errorMessage !== "" && (
								<Alert
									type="error"
									title="Oh no! Something did not go as planned."
									message={errorMessage}
								/>
							)}
						</div>
						<div>
							<MySurveys loading={isLoading} />
						</div>
					</div>
				)}
			</div>
			<div className="shrink-0">
				<Footer />
			</div>
		</div>
	);
};

export default App;
