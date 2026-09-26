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

const App = (): React.JSX.Element => {
	const [errorMessage, setErrorMessage] = useState("");
	const [newSurveyId, setNewSurveyId] = useState<number | null>(null);
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
			} catch {
				setErrorMessage("Oops, something went wrong with registering a user.");
				return;
			}
		};

		const register = async () => {
			if (isAuthenticated && user) {
				await registerUser();
			}
		};

		register();
	}, [isAuthenticated, user, getAccessTokenSilently]);

	return (
		<div className="flex min-h-screen flex-col">
			<div className="flex-1 w-full max-w-[1600px] mx-auto px-4 sm:px-6 lg:px-8">
				<NavBar />
				{!isAuthenticated && !isLoading ? (
					<Splash />
				) : (
					<div className="grid grid-cols-1 md:grid-cols-2 gap-5 lg:gap-6 my-5 lg:my-6">
						<div>
							<CreateSurvey
								loading={isLoading}
								onSurveyCreated={setNewSurveyId}
							/>
						</div>
						<div>
							<GetSurvey loading={isLoading} newSurveyId={newSurveyId} />
							{errorMessage !== "" && (
								<Alert
									type="error"
									title="Oh no! Something did not go as planned."
									message={errorMessage}
								/>
							)}
						</div>
						<div className="md:col-span-2">
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
