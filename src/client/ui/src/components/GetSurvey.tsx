import { faPaperPlane } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import type React from "react";
import { useEffect, useRef, useState } from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import { useSurveyFetch } from "../hooks";
import Alert from "./Alert";
import Field from "./Field";
import SkeletonButton from "./SkeletonButton";
import SurveyResult from "./SurveyResult";

export type GetSurveyProps = {
	loading: boolean;
	newSurveyId: number | null;
};

const GetSurvey = ({ loading, newSurveyId }: GetSurveyProps) => {
	const [surveyIdInput, setSurveyIdInput] = useState(0);
	const formRef = useRef<HTMLFormElement>(null);
	const [triggerFetch, setTriggerFetch] = useState<number | null>(null);
	const {
		survey: surveyDetail,
		loading: isSearching,
		error: errorMessage,
		refetch,
	} = useSurveyFetch(triggerFetch);

	// Auto-fetch when newSurveyId changes (from CreateSurvey)
	useEffect(() => {
		if (newSurveyId) {
			setSurveyIdInput(newSurveyId);
			setTriggerFetch(newSurveyId);
		}
	}, [newSurveyId]);

	const submitForm = async (e: React.FormEvent) => {
		e.preventDefault();
		if (surveyIdInput === triggerFetch && surveyIdInput > 0) {
			void refetch();
		} else {
			setTriggerFetch(surveyIdInput);
		}
	};

	return (
		<SkeletonTheme baseColor="#30353a" highlightColor="#c7ff18">
			<div className="space-y-5">
				<section className="brutal-panel">
					<h2 className="display-title mb-6 text-4xl lg:text-5xl">
						{loading ? <Skeleton width={100} /> : <span>Get Survey</span>}
					</h2>
					<form ref={formRef} onSubmit={submitForm} className="space-y-2">
						<div className="mb-3">
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
								loading={loading}
								type="submit"
								disabled={isSearching}
								additionalClasses={
									isSearching ? ["opacity-80", "cursor-not-allowed"] : []
								}
							>
								{isSearching ? "Searching..." : "Get Survey"}
								<FontAwesomeIcon icon={faPaperPlane} />
							</SkeletonButton>
						</div>
					</form>
				</section>
				{surveyDetail && surveyDetail.id > 0 && (
					<div>
						<SurveyResult
							surveyDetail={surveyDetail}
							fallbackFocus={() =>
								formRef.current?.querySelector("input") ?? null
							}
							onDeleted={() => {
								setSurveyIdInput(0);
								setTriggerFetch(null);
							}}
						/>
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
