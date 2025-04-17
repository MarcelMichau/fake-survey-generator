import type { SurveyModel } from "../types";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
	faCalendarAlt,
	faPoll,
	faUsers,
	faTrophy,
	faChartBar,
} from "@fortawesome/free-solid-svg-icons";

type SurveyResultProps = {
	surveyDetail: SurveyModel;
};

const SurveyResult = ({ surveyDetail }: SurveyResultProps) => {
	return (
		<div className="w-full animate-fade-in">
			<div className="dark:bg-gray-800/80 backdrop-blur-sm border dark:border-gray-600/60 rounded-lg p-6 flex flex-col justify-between leading-normal shadow-lg">
				<div className="mb-6">
					<div className="flex items-center mb-4">
						<FontAwesomeIcon
							icon={faPoll}
							className="text-indigo-500 mr-3 text-xl"
						/>
						<h3 className="text-lg font-semibold text-gray-200">
							Survey Results
						</h3>
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
			</div>
		</div>
	);
};

export default SurveyResult;
