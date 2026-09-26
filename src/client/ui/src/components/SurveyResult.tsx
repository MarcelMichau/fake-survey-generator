import { useState } from "react";
import { useAuth0 } from "@auth0/auth0-react";
import type { SurveyModel } from "../types";
import { useApiCall } from "../hooks";
import ConfirmDialog from "./ConfirmDialog";
import Alert from "./Alert";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
	faCalendarAlt,
	faUsers,
	faTrophy,
	faChartBar,
	faTrash,
} from "@fortawesome/free-solid-svg-icons";

type SurveyResultProps = {
	surveyDetail: SurveyModel;
	fallbackFocus?: () => HTMLElement | null;
	onDeleted?: (id: number) => void;
};

const SurveyResult = ({
	surveyDetail,
	fallbackFocus,
	onDeleted,
}: SurveyResultProps) => {
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
		<>
			<section className="brutal-panel" aria-label="Survey Results">
				<div className="mb-5 flex items-center justify-between gap-3">
					<h3 className="display-title text-4xl lg:text-5xl">Survey Results</h3>
					{isOwner && (
						<button
							type="button"
							aria-label="Delete this survey"
							onClick={() => setConfirmOpen(true)}
							className="brutal-icon-button shrink-0"
						>
							<FontAwesomeIcon icon={faTrash} />
						</button>
					)}
				</div>
				<div className="mb-4 flex items-start gap-3 border-2 border-paper bg-surface px-3 py-3">
					<FontAwesomeIcon icon={faUsers} className="mt-1 shrink-0" />
					<p>
						This survey asked{" "}
						<strong>
							{new Intl.NumberFormat().format(surveyDetail.numberOfRespondents)}
						</strong>{" "}
						<strong>{surveyDetail.respondentType}</strong> the question:
					</p>
				</div>
				<div className="display-title mb-4 border-l-[8px] border-paper bg-surface px-4 py-3 text-2xl sm:text-3xl">
					{surveyDetail.topic}
				</div>
				<p className="mb-3 flex items-center gap-3">
					<FontAwesomeIcon icon={faChartBar} /> And the results were:
				</p>
				<div className="space-y-2">
					{[...surveyDetail.options]
						.sort((x, y) => y.numberOfVotes - x.numberOfVotes)
						.map((option, index) => (
							<div
								key={option.optionText}
								className="flex flex-wrap items-center gap-2 border-2 border-paper bg-surface text-base"
							>
								<span
									className={`flex min-w-16 self-stretch items-center justify-center gap-2 border-r-2 border-paper px-3 py-2 font-bold ${index === 0 ? "bg-lime text-ink" : ""}`}
								>
									{index === 0 && <FontAwesomeIcon icon={faTrophy} />} #
									{index + 1}
								</span>
								<span className="min-w-0 flex-1 px-2 py-2 wrap-break-word">
									{option.optionText}
								</span>
								<span
									className={`m-1 px-3 py-1 font-bold whitespace-nowrap ${index === 0 ? "bg-lime text-ink" : "bg-[#454a4f] text-paper"}`}
								>
									{new Intl.NumberFormat().format(option.numberOfVotes)} votes
								</span>
							</div>
						))}
				</div>
				<p className="mt-5 flex items-center gap-3 border-t-2 border-paper pt-4 text-sm">
					<FontAwesomeIcon icon={faCalendarAlt} />
					{new Intl.DateTimeFormat("default", {
						weekday: "long",
						year: "numeric",
						month: "long",
						day: "numeric",
					}).format(new Date(surveyDetail.createdOn))}
				</p>
				{error && (
					<Alert
						type="error"
						title="Oh no! Something did not go as planned."
						message={error}
					/>
				)}
			</section>
			<ConfirmDialog
				open={confirmOpen}
				title="Delete survey?"
				message={`This will permanently delete "${surveyDetail.topic}". This action cannot be undone.`}
				confirmLabel="Delete"
				busy={isDeleting}
				fallbackFocus={fallbackFocus}
				onConfirm={confirmDelete}
				onCancel={() => {
					if (!isDeleting) setConfirmOpen(false);
				}}
			/>
		</>
	);
};

export default SurveyResult;
