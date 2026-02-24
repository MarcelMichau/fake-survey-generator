import type React from "react";
import { useState, useCallback } from "react";
import Skeleton, { SkeletonTheme } from "react-loading-skeleton";
import type * as Types from "../types";
import SkeletonButton from "./SkeletonButton";
import Alert from "./Alert";
import { useApiCall } from "../hooks";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faPaperPlane } from "@fortawesome/free-solid-svg-icons";

export type MySurveysProps = {
	loading: boolean;
};

const MySurveys = ({ loading }: MySurveysProps) => {
	const { apiCall } = useApiCall();
	const [userSurveys, setUserSurveys] = useState<Types.UserSurveyModel[]>([]);
	const [isSearching, setIsSearching] = useState(false);
	const [hasFetched, setHasFetched] = useState(false);
	const [error, setError] = useState<string | null>(null);

	const fetchSurveys = useCallback(async () => {
		setIsSearching(true);
		setError(null);

		try {
			const response = await apiCall("api/survey/user");

			if (!response.ok) {
				setError("Failed to fetch surveys");
				setUserSurveys([]);
				return;
			}

			const data: Types.UserSurveyModel[] = await response.json();
			setUserSurveys(data);
		} catch (err) {
			setError(
				err instanceof Error ? err.message : "An unexpected error occurred",
			);
			setUserSurveys([]);
		} finally {
			setIsSearching(false);
			setHasFetched(true);
		}
	}, [apiCall]);

	const submitForm = async (e: React.FormEvent) => {
		e.preventDefault();
		await fetchSurveys();
	};

	const tablePadding = "px-4 py-2";
	const tableBorder = "border border-gray-700";

	type TableHeaderProps = { children: React.ReactNode };
	const TableHeader = ({ children }: TableHeaderProps) => (
		<th className={`${tablePadding}`}>{children}</th>
	);

	type TableDataProps = { children: React.ReactNode };
	const TableData = ({ children }: TableDataProps) => (
		<td className={`${tableBorder} ${tablePadding}`}>{children}</td>
	);

	return (
		<SkeletonTheme baseColor="#2d3748" highlightColor="#667eea">
			<div className="dark:bg-gray-800 rounded-sm px-8 pt-6 pb-8 mb-4">
				<h2 className="dark:text-indigo-500 text-xl font-semibold tracking-tight mb-2">
					{loading ? <Skeleton width={100} /> : <span>My Surveys</span>}
				</h2>
				<form onSubmit={submitForm}>
					<SkeletonButton
						onClick={submitForm}
						loading={loading}
						type="submit"
						additionalClasses={
							isSearching ? ["opacity-80", "cursor-not-allowed"] : []
						}
					>
						{isSearching ? "Searching..." : "Get My Surveys"}
						<FontAwesomeIcon icon={faPaperPlane} className="ml-1" />
					</SkeletonButton>
				</form>

				{userSurveys.length > 0 && (
					<table
						className={`table-auto bg-gray-900 text-gray-400 ${tableBorder} my-4`}
					>
						<thead>
							<tr>
								<TableHeader>Question</TableHeader>
								<TableHeader>Audience</TableHeader>
								<TableHeader># Respondents</TableHeader>
								<TableHeader># Options</TableHeader>
								<TableHeader>Winning Option</TableHeader>
								<TableHeader>Winning # Votes</TableHeader>
							</tr>
						</thead>
						<tbody>
							{userSurveys.map((survey) => (
								<tr key={survey.id}>
									<TableData>{survey.topic}</TableData>
									<TableData>{survey.respondentType}</TableData>
									<TableData>
										{new Intl.NumberFormat().format(survey.numberOfRespondents)}
									</TableData>
									<TableData>{survey.numberOfOptions}</TableData>
									<TableData>{survey.winningOption}</TableData>
									<TableData>
										{new Intl.NumberFormat().format(
											survey.winningOptionNumberOfVotes,
										)}
									</TableData>
								</tr>
							))}
						</tbody>
					</table>
				)}
				{hasFetched && userSurveys?.length === 0 && (
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
			</div>
		</SkeletonTheme>
	);
};

export default MySurveys;
