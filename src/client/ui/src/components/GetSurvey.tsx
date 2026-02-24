import type React from "react";
import { useState, useEffect } from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import Field from "./Field";
import SkeletonButton from "./SkeletonButton";
import Alert from "./Alert";
import SurveyResult from "./SurveyResult";
import { useSurveyFetch } from "../hooks";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPaperPlane, faSearch } from "@fortawesome/free-solid-svg-icons";

export type GetSurveyProps = {
	loading: boolean;
	newSurveyId: number | null;
};

const GetSurvey = ({ loading, newSurveyId }: GetSurveyProps) => {
	const [surveyIdInput, setSurveyIdInput] = useState(0);
	const [triggerFetch, setTriggerFetch] = useState<number | null>(null);
	const { survey: surveyDetail, loading: isSearching, error: errorMessage } = useSurveyFetch(triggerFetch);

	// Auto-fetch when newSurveyId changes (from CreateSurvey)
	useEffect(() => {
		if (newSurveyId) {
			setSurveyIdInput(newSurveyId);
			setTriggerFetch(newSurveyId);
		}
	}, [newSurveyId]);

	const submitForm = async (e: React.FormEvent) => {
		e.preventDefault();
		setTriggerFetch(surveyIdInput);
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
							value={surveyIdInput}
							onChange={(value) =>
								setSurveyIdInput(
									Number.isNaN(Number(value)) ? surveyIdInput : Number(value),
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

				{surveyDetail && surveyDetail.id > 0 && (
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
