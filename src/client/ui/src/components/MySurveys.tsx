import { faPaperPlane, faTrash } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import type React from "react";
import { useMemo, useRef, useState } from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import { useDeleteSurvey, useUserSurveys } from "../hooks/useSurveys";
import type * as Types from "../types";
import Alert from "./Alert";
import ConfirmDialog from "./ConfirmDialog";
import SkeletonButton from "./SkeletonButton";

export type MySurveysProps = {
	loading: boolean;
};

const MySurveys = ({ loading }: MySurveysProps) => {
	const formRef = useRef<HTMLFormElement>(null);
	const [hasFetched, setHasFetched] = useState(false);
	const query = useUserSurveys(hasFetched);
	const deletion = useDeleteSurvey();
	const userSurveys = query.isError ? [] : (query.data ?? []);
	const isSearching = query.isFetching;
	const isDeleting = deletion.isPending;
	const error = deletion.error?.message ?? query.error?.message;
	const [surveyToDelete, setSurveyToDelete] =
		useState<Types.UserSurveyModel | null>(null);
	const numberFormatter = useMemo(() => new Intl.NumberFormat(), []);

	const submitForm = (e: React.SubmitEvent<HTMLFormElement>) => {
		e.preventDefault();
		deletion.reset();
		if (hasFetched) void query.refetch();
		else setHasFetched(true);
	};

	const confirmDelete = async () => {
		if (!surveyToDelete) return;
		try {
			await deletion.mutateAsync(surveyToDelete.id);
			setSurveyToDelete(null);
		} catch {
			// The mutation exposes the failure and leaves the dialog open for retry.
		}
	};

	return (
		<SkeletonTheme baseColor="#30353a" highlightColor="#c7ff18">
			<div className="brutal-panel">
				<div className="flex flex-wrap items-center justify-between gap-4 mb-2">
					<h2 className="display-title text-4xl lg:text-5xl">
						{loading ? <Skeleton width={100} /> : <span>My Surveys</span>}
					</h2>
					<form ref={formRef} onSubmit={submitForm}>
						<SkeletonButton
							loading={loading}
							type="submit"
							disabled={isSearching}
							additionalClasses={
								isSearching ? ["opacity-80", "cursor-not-allowed"] : []
							}
						>
							{isSearching ? "Searching..." : "Get My Surveys"}
							<FontAwesomeIcon icon={faPaperPlane} className="ml-1" />
						</SkeletonButton>
					</form>
				</div>

				{userSurveys.length > 0 && (
					<div className="overflow-x-auto mt-5">
						<table className="brutal-table min-w-[850px]">
							<thead>
								<tr>
									<th>Question</th>
									<th>Audience</th>
									<th># Respondents</th>
									<th># Options</th>
									<th>Winning Option</th>
									<th>Winning # Votes</th>
									<th>Actions</th>
								</tr>
							</thead>
							<tbody>
								{userSurveys.map((survey) => (
									<tr key={survey.id}>
										<td>{survey.topic}</td>
										<td>{survey.respondentType}</td>
										<td>
											{numberFormatter.format(survey.numberOfRespondents)}
										</td>
										<td>{survey.numberOfOptions}</td>
										<td>{survey.winningOption}</td>
										<td>
											{numberFormatter.format(
												survey.winningOptionNumberOfVotes,
											)}
										</td>
										<td>
											<button
												type="button"
												aria-label={`Delete survey ${survey.topic}`}
												onClick={() => {
													deletion.reset();
													setSurveyToDelete(survey);
												}}
												className="brutal-icon-button"
											>
												<FontAwesomeIcon icon={faTrash} />
											</button>
										</td>
									</tr>
								))}
							</tbody>
						</table>
					</div>
				)}
				{query.isSuccess && userSurveys.length === 0 && (
					<Alert
						title="No Surveys"
						message={"You have not created any surveys yet. :("}
					/>
				)}
				{error && (
					<Alert
						type="error"
						title="Oh no! Something did not go as planned."
						message={error}
					/>
				)}

				<ConfirmDialog
					open={surveyToDelete !== null}
					title="Delete survey?"
					message={
						surveyToDelete
							? `This will permanently delete "${surveyToDelete.topic}". This action cannot be undone.`
							: ""
					}
					confirmLabel="Delete"
					busy={isDeleting}
					fallbackFocus={() => formRef.current?.querySelector("button") ?? null}
					onConfirm={confirmDelete}
					onCancel={() => {
						if (!isDeleting) setSurveyToDelete(null);
					}}
				/>
			</div>
		</SkeletonTheme>
	);
};

export default MySurveys;
