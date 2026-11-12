// biome-ignore-all lint/a11y/noNoninteractiveTabindex: the named table overflow region needs consistent keyboard scrolling across browsers.
import { faPaperPlane, faTrash } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import type React from "react";
import { useId, useMemo, useRef, useState } from "react";
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
	const deleteTrigger = useRef<HTMLButtonElement>(null);
	const id = useId();
	const [hasFetched, setHasFetched] = useState(false);
	const query = useUserSurveys(hasFetched);
	const deletion = useDeleteSurvey();
	const userSurveys = query.isError ? [] : (query.data ?? []);
	const isSearching = query.isFetching;
	const isDeleting = deletion.isPending;
	const error = query.error?.message;
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
				<div className="flex flex-wrap items-center justify-between gap-4">
					<h2 id={`${id}-title`} className="display-title text-4xl lg:text-5xl">
						{loading ? <Skeleton width={100} /> : <span>My Surveys</span>}
					</h2>
					<form
						ref={formRef}
						onSubmit={submitForm}
						aria-labelledby={`${id}-title`}
					>
						<SkeletonButton
							loading={loading}
							type="submit"
							disabled={isSearching}
							additionalClasses={
								isSearching ? ["opacity-80", "cursor-not-allowed"] : []
							}
						>
							{isSearching ? "Searching..." : "Get My Surveys"}
							<FontAwesomeIcon icon={faPaperPlane} />
						</SkeletonButton>
					</form>
				</div>

				<p role="status" className="ui-hint mt-4">
					{query.isSuccess && !isSearching
						? `${userSurveys.length} ${userSurveys.length === 1 ? "survey" : "surveys"} loaded.`
						: ""}
				</p>
				{userSurveys.length > 0 && (
					<section
						className="survey-table-wrapper mt-6"
						tabIndex={0}
						aria-labelledby={`${id}-caption`}
					>
						<table className="brutal-table min-w-[850px]">
							<caption id={`${id}-caption`} className="sr-only">
								Your surveys
							</caption>
							<thead>
								<tr>
									<th scope="col">Question</th>
									<th scope="col">Audience</th>
									<th scope="col" className="is-numeric">
										# Respondents
									</th>
									<th scope="col" className="is-numeric">
										# Options
									</th>
									<th scope="col">Winning Option</th>
									<th scope="col" className="is-numeric">
										Winning # Votes
									</th>
									<th scope="col" className="w-px">
										Actions
									</th>
								</tr>
							</thead>
							<tbody>
								{userSurveys.map((survey) => (
									<tr key={survey.id}>
										<th scope="row">{survey.topic}</th>
										<td>{survey.respondentType}</td>
										<td className="is-numeric">
											{numberFormatter.format(survey.numberOfRespondents)}
										</td>
										<td className="is-numeric">{survey.numberOfOptions}</td>
										<td>{survey.winningOption}</td>
										<td className="is-numeric">
											{numberFormatter.format(
												survey.winningOptionNumberOfVotes,
											)}
										</td>
										<td className="text-center">
											<button
												type="button"
												aria-label={`Delete survey ${survey.topic}`}
												onClick={(event) => {
													deleteTrigger.current = event.currentTarget;
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
					</section>
				)}
				{query.isSuccess && userSurveys.length === 0 && (
					<Alert
						title="No Surveys"
						announcement="off"
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
					invoker={deleteTrigger.current}
					error={deletion.error?.message}
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
