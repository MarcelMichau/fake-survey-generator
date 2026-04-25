import { useState } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import type { SurveyModel } from "../types";
import { useApiCall } from "../hooks";
import ConfirmDialog from "./ConfirmDialog";
import Alert from "./Alert";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
	faCalendarAlt,
	faPoll,
	faUsers,
	faTrophy,
	faChartBar,
	faTrash,
} from "@fortawesome/free-solid-svg-icons";

type SurveyResultProps = {
	surveyDetail: SurveyModel;
	onDeleted?: (id: number) => void;
};

const SurveyResult = ({ surveyDetail, onDeleted }: SurveyResultProps) => {
	const { user } = useAuth0();
	const { apiCall } = useApiCall();
	const [confirmOpen, setConfirmOpen] = useState(false);
	const [isDeleting, setIsDeleting] = useState(false);
	const [error, setError] = useState<string | null>(null);

	const isOwner = !!user?.sub && user.sub === surveyDetail.ownerExternalUserId;

	const confirmDelete = async () => {
		setIsDeleting(true);
		setError(null);
		try {
			const response = await apiCall(`api/survey/${surveyDetail.id}`, {
				method: "DELETE",
			});

			if (!response.ok) {
				setError("Failed to delete survey");
				return;
			}

			setConfirmOpen(false);
			onDeleted?.(surveyDetail.id);
		} catch (err) {
			setError(
				err instanceof Error ? err.message : "An unexpected error occurred",
			);
		} finally {
			setIsDeleting(false);
		}
	};

	return (
		<div className="w-full animate-fade-in">
			<div className="dark:bg-gray-800/80 backdrop-blur-sm border dark:border-gray-600/60 rounded-lg p-6 flex flex-col justify-between leading-normal shadow-lg">
				<div className="mb-6">
					<div className="flex items-center justify-between mb-4">
						<div className="flex items-center">
							<FontAwesomeIcon
								icon={faPoll}
								className="text-indigo-500 mr-3 text-xl"
							/>
							<h3 className="text-lg font-semibold text-gray-200">
								Survey Results
							</h3>
						</div>
						{isOwner && (
							<button
								type="button"
								aria-label="Delete this survey"
								onClick={() => setConfirmOpen(true)}
								className="text-red-400 hover:text-red-300 transition-colors px-2 py-1"
							>
								<FontAwesomeIcon icon={faTrash} />
							</button>
						)}
					</div>

					<div className="flex items-center mb-4 bg-gray-700/50 px-4 py-3 rounded-md">
						<FontAwesomeIcon icon={faUsers} className="text-blue-400 mr-3" />
						<p className="text-gray-300">
							This survey asked{" "}
							<span className="font-bold text-white">
								{new Intl.NumberFormat().format(
									surveyDetail.numberOfRespondents,
								)}
							</span>{" "}
							<span className="font-bold text-indigo-400">
								{surveyDetail.respondentType}
							</span>{" "}
							the question:
						</p>
					</div>

					<div className="text-white font-bold text-xl mb-4 border-l-4 border-indigo-500 pl-4 py-2">
						{surveyDetail.topic}
					</div>

					<p className="text-sm text-gray-300 flex items-center mb-4">
						<FontAwesomeIcon
							icon={faChartBar}
							className="text-indigo-400 mr-2"
						/>
						And the results were:
					</p>

					<div className="text-gray-300 text-base space-y-3">
						{surveyDetail.options
							.sort((x, y) => y.numberOfVotes - x.numberOfVotes)
							.map((option, index) => (
								<div
									key={option.id}
									className={`flex items-center p-3 rounded-md ${
										index === 0
											? "bg-gradient-to-r from-green-900/40 to-green-800/20 border-l-4 border-green-500"
											: index === 1
												? "bg-gradient-to-r from-blue-900/40 to-blue-800/20 border-l-4 border-blue-500"
												: "bg-gray-700/30 border-l-4 border-gray-600"
									} transition-all duration-200 hover:shadow-md card-hover`}
								>
									{index === 0 && (
										<FontAwesomeIcon
											icon={faTrophy}
											className="text-yellow-400 mr-3"
										/>
									)}
									<span
										className={`mr-3 ${index === 0 ? "text-white" : "text-gray-400"}`}
									>
										#{index + 1}
									</span>
									<span className="flex-grow">{option.optionText}</span>
									<span
										className={`ml-2 inline-block ${
											index === 0
												? "bg-green-900/80 text-green-300 border-green-700"
												: index === 1
													? "bg-blue-900/80 text-blue-300 border-blue-700"
													: "bg-gray-700 text-gray-300 border-gray-600"
										} rounded-full px-4 py-1 text-sm font-medium border`}
									>
										{new Intl.NumberFormat().format(option.numberOfVotes)} votes
									</span>
								</div>
							))}
					</div>
				</div>

				<div className="text-sm border-t border-gray-700 pt-4 mt-2">
					<p className="text-gray-400 flex items-center">
						<FontAwesomeIcon
							icon={faCalendarAlt}
							className="mr-2 text-indigo-400"
						/>
						{new Intl.DateTimeFormat("default", {
							weekday: "long",
							year: "numeric",
							month: "long",
							day: "numeric",
						}).format(new Date(surveyDetail.createdOn))}
					</p>
				</div>

				{error && (
					<div className="mt-4">
						<Alert
							type="error"
							title="Oh no! Something did not go as planned."
							message={error}
						/>
					</div>
				)}
			</div>

			<ConfirmDialog
				open={confirmOpen}
				title="Delete survey?"
				message={`This will permanently delete "${surveyDetail.topic}". This action cannot be undone.`}
				confirmLabel="Delete"
				busy={isDeleting}
				onConfirm={confirmDelete}
				onCancel={() => {
					if (!isDeleting) setConfirmOpen(false);
				}}
			/>
		</div>
	);
};

export default SurveyResult;
