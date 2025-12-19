import type React from "react";
import { useState, useEffect } from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import { useAuth0 } from "@auth0/auth0-react";
import type * as Types from "../types";
import Field from "./Field";
import SkeletonButton from "./SkeletonButton";
import Alert from "./Alert";
import SurveyResult from "./SurveyResult";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPaperPlane, faSearch } from "@fortawesome/free-solid-svg-icons";

export type GetSurveyProps = {
	loading: boolean;
	newSurveyId: number | null;
};

const GetSurvey = ({ loading, newSurveyId }: GetSurveyProps) => {
	const { getAccessTokenSilently } = useAuth0();
	const [surveyId, setSurveyId] = useState(0);
	const [surveyDetail, setSurveyDetail] = useState({} as Types.SurveyModel);
	const [errorMessage, setErrorMessage] = useState("");
	const [isSearching, setIsSearching] = useState(false);

	useEffect(() => {
		if (newSurveyId) {
			setSurveyId(newSurveyId);
			fetchSurvey(newSurveyId);
		}
	}, [newSurveyId]);

	const resetMessage = (): void => {
		setErrorMessage("");
	};

	const fetchSurvey = async (surveyId: number) => {
		resetMessage();
		setIsSearching(true);

		try {
			const token = await getAccessTokenSilently();

			const response = await fetch(`api/survey/${surveyId}`, {
				headers: {
					Authorization: `Bearer ${token}`,
				},
			});

			if (response.status === 404) {
				setErrorMessage("Looks like that survey does not exist");
				setSurveyDetail({} as Types.SurveyModel);
				return;
			}

			if (response.status !== 200) {
				setErrorMessage("Something did not go as planned");
				setSurveyDetail({} as Types.SurveyModel);
				return;
			}

			const survey: Types.SurveyModel = await response.json();
			setSurveyDetail(survey);
		} finally {
			setIsSearching(false);
		}
	};

	const submitForm = async (e: React.FormEvent) => {
		e.preventDefault();
		await fetchSurvey(surveyId);
	};

	return (
		<SkeletonTheme baseColor="#2d3748" highlightColor="#667eea">
			<div className="dark:bg-gray-800/90 backdrop-blur-sm rounded-lg px-8 pt-6 pb-8 mb-6 shadow-lg border border-gray-700/60">
				<h2 className="dark:text-indigo-400 text-xl font-semibold tracking-tight mb-4 flex items-center">
					{loading ? (
						<Skeleton width={100} />
					) : (
						<>
							<FontAwesomeIcon
								icon={faSearch}
								className="mr-2 text-indigo-500"
							/>
							<span>Get Survey</span>
						</>
					)}
				</h2>
				<form onSubmit={submitForm} className="space-y-4">
					<div className="mb-4">
						<Field
							label="Survey ID"
							value={surveyId}
							onChange={(value) =>
								setSurveyId(
									Number.isNaN(Number(value)) ? surveyId : Number(value),
								)
							}
							loading={loading}
							placeholder="Enter survey ID number"
						/>
					</div>
					<div className="flex justify-start">
						<SkeletonButton
							onClick={submitForm}
							loading={loading}
							type="submit"
							additionalClasses={
								isSearching ? ["opacity-80", "cursor-not-allowed"] : []
							}
						>
							{isSearching ? "Searching..." : "Get Survey"}
							<FontAwesomeIcon icon={faPaperPlane} className="ml-2" />
						</SkeletonButton>
					</div>
				</form>

				{surveyDetail.id > 0 && (
					<div className="mt-8 animate-fade-in">
						<SurveyResult surveyDetail={surveyDetail} />
					</div>
				)}

				{errorMessage !== "" && (
					<div className="mt-6">
						<Alert
							type="error"
							title="Oh no! Something did not go as planned."
							message={errorMessage}
						/>
					</div>
				)}
			</div>
		</SkeletonTheme>
	);
};

export default GetSurvey;
