import { useAuth0 } from "@auth0/auth0-react";
import type React from "react";
import { useState } from "react";
import Alert from "./components/Alert";
import Button from "./components/Button";
import CreateSurvey from "./components/CreateSurvey";
import Footer from "./components/Footer";
import GetSurvey from "./components/GetSurvey";
import MySurveys from "./components/MySurveys";
import NavBar from "./components/NavBar";
import Splash from "./components/Splash";
import { useUserRegistration } from "./hooks/useUserRegistration";

const App = (): React.JSX.Element => {
	const [newSurveyId, setNewSurveyId] = useState<number | null>(null);
	const { isAuthenticated, isLoading } = useAuth0();
	const registration = useUserRegistration();
	const loading = isLoading || !registration.isSuccess;

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
								loading={loading}
								onSurveyCreated={setNewSurveyId}
							/>
						</div>
						<div>
							<GetSurvey loading={loading} newSurveyId={newSurveyId} />
							{registration.isError && (
								<div className="space-y-3">
									<Alert
										type="error"
										title="Oh no! Something did not go as planned."
										message={registration.error.message}
									/>
									<Button
										actionType="secondary"
										onClick={() => void registration.refetch()}
										disabled={registration.isFetching}
									>
										Retry registration
									</Button>
								</div>
							)}
						</div>
						<div className="md:col-span-2">
							<MySurveys loading={loading} />
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
