import { faPaperPlane } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import type React from "react";
import { useEffect, useId, useRef, useState } from "react";
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
	const [surveyIdInput, setSurveyIdInput] = useState("");
	const id = useId();
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
			setSurveyIdInput(String(newSurveyId));
			setTriggerFetch(newSurveyId);
		}
	}, [newSurveyId]);

	const submitForm = async (e: React.FormEvent) => {
		e.preventDefault();
		const surveyId = Number(surveyIdInput);
		if (surveyId === triggerFetch && surveyId > 0) {
			void refetch();
		} else {
			setTriggerFetch(surveyId);
		}
	};

	return (
		<SkeletonTheme baseColor="#30353a" highlightColor="#c7ff18">
			<div className="space-y-5">
				<section className="brutal-panel">
					<h2
						id={`${id}-title`}
						className="display-title mb-6 text-4xl lg:text-5xl"
					>
						{loading ? <Skeleton width={100} /> : <span>Get Survey</span>}
					</h2>
					<search>
						<form
							ref={formRef}
							onSubmit={submitForm}
							className="space-y-2"
							aria-labelledby={`${id}-title`}
						>
							<div className="mb-3">
								<Field
									label="Survey ID"
									name="surveyId"
									inputMode="numeric"
									required
									pattern="[1-9][0-9]*"
									maxLength={10}
									enterKeyHint="search"
									autoCorrect="off"
									autoCapitalize="none"
									spellCheck={false}
									hint="Enter a positive whole-number survey ID."
									value={surveyIdInput}
									onChange={setSurveyIdInput}
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
					</search>
				</section>
				{surveyDetail && surveyDetail.id > 0 && (
					<div>
						<SurveyResult
							surveyDetail={surveyDetail}
							fallbackFocus={() =>
								formRef.current?.querySelector("input") ?? null
							}
							onDeleted={() => {
								setSurveyIdInput("");
								setTriggerFetch(null);
							}}
						/>
					</div>
				)}
				<p role="status" className="ui-hint">
					{surveyDetail && !isSearching
						? `Survey ${surveyDetail.id} loaded.`
						: ""}
				</p>

				{errorMessage !== "" && (
					<div className="mt-6">
						<Alert
							type="error"
							announcement="polite"
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
