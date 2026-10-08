import { useAuth0 } from "@auth0/auth0-react";
import {
	faCalendarAlt,
	faChartBar,
	faTrash,
	faTrophy,
	faUsers,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { useState } from "react";
import { useDeleteSurvey } from "../hooks/useSurveys";
import type { SurveyModel } from "../types";
import Alert from "./Alert";
import ConfirmDialog from "./ConfirmDialog";

const shareFormatter = new Intl.NumberFormat(undefined, {
	style: "percent",
	maximumFractionDigits: 1,
});

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
	const deletion = useDeleteSurvey();
	const [confirmOpen, setConfirmOpen] = useState(false);
	const isDeleting = deletion.isPending;
	const error = deletion.error?.message;
	const isOwner = !!user?.sub && user.sub === surveyDetail.ownerExternalUserId;
	const totalVotes = surveyDetail.options.reduce(
		(total, option) => total + option.numberOfVotes,
		0,
	);

	const confirmDelete = async () => {
		try {
			await deletion.mutateAsync(surveyDetail.id);
			setConfirmOpen(false);
			onDeleted?.(surveyDetail.id);
		} catch {
			// The mutation exposes the failure and leaves the dialog open for retry.
		}
	};

	return (
		<>
			<section className="brutal-panel" aria-label="Survey Results">
				<div className="mb-6 flex items-center justify-between gap-3">
					<h3 className="display-title text-4xl lg:text-5xl">Survey Results</h3>
					{isOwner && (
						<button
							type="button"
							aria-label="Delete this survey"
							onClick={() => {
								deletion.reset();
								setConfirmOpen(true);
							}}
							className="brutal-icon-button shrink-0"
						>
							<FontAwesomeIcon icon={faTrash} />
						</button>
					)}
				</div>
				<p className="mb-3 flex items-start gap-3 text-muted">
					<FontAwesomeIcon icon={faUsers} className="mt-1 shrink-0" />
					<span>
						This survey asked{" "}
						<strong className="text-paper">
							{new Intl.NumberFormat().format(surveyDetail.numberOfRespondents)}
						</strong>{" "}
						<strong className="text-paper">
							{surveyDetail.respondentType}
						</strong>{" "}
						the question:
					</span>
				</p>
				<div className="display-title mb-6 border-l-[6px] border-lime bg-surface px-4 py-3 text-2xl sm:text-3xl">
					{surveyDetail.topic}
				</div>
				<p className="mb-3 flex items-center gap-3 text-muted">
					<FontAwesomeIcon icon={faChartBar} /> And the results were:
				</p>
				<div className="space-y-2">
					{[...surveyDetail.options]
						.sort((x, y) => y.numberOfVotes - x.numberOfVotes)
						.map((option, index) => {
							const isWinner = index === 0;
							const share =
								totalVotes > 0 ? (option.numberOfVotes / totalVotes) * 100 : 0;

							return (
								<div
									key={option.optionText}
									className={`relative flex items-stretch border-2 bg-surface text-base ${isWinner ? "border-lime" : "border-line"}`}
								>
									<span
										aria-hidden="true"
										className={`absolute inset-y-0 left-0 ${isWinner ? "bg-lime/20" : "bg-paper/8"}`}
										style={{ width: `${share}%` }}
									/>
									<span
										className={`relative flex min-w-16 items-center justify-center gap-2 px-3 py-2.5 font-bold ${isWinner ? "bg-lime text-ink" : "border-r-2 border-line"}`}
									>
										{isWinner && <FontAwesomeIcon icon={faTrophy} />}#
										{index + 1}
									</span>
									<span className="relative min-w-0 flex-1 self-center px-3 py-2.5 wrap-break-word">
										{option.optionText}
									</span>
									<span className="relative flex flex-col items-end justify-center px-3 py-1.5 text-right tabular-nums">
										<span className="font-bold whitespace-nowrap">
											{new Intl.NumberFormat().format(option.numberOfVotes)}{" "}
											votes
										</span>
										<span className="text-xs text-muted">
											{shareFormatter.format(share / 100)}
										</span>
									</span>
								</div>
							);
						})}
				</div>
				<p className="mt-6 flex items-center gap-3 border-t-2 border-line pt-4 text-sm text-muted">
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
