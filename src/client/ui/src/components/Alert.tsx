import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
	faCircleCheck,
	faCircleExclamation,
} from "@fortawesome/free-solid-svg-icons";

type AlertType = "error" | "success";

type AlertProps = {
	title: string;
	message: string;
	type?: AlertType;
};

const Alert = ({ title, message, type = "success" }: AlertProps) => {
	const styleMap = {
		success: {
			containerClasses: "bg-green-800/80 border-green-500 backdrop-blur-sm",
			iconClasses: "text-green-400",
			icon: faCircleCheck,
		},
		error: {
			containerClasses: "bg-red-800/80 border-red-500 backdrop-blur-sm",
			iconClasses: "text-red-400",
			icon: faCircleExclamation,
		},
	};

	const { containerClasses, iconClasses, icon } = styleMap[type];

	return (
		<div
			className={`${containerClasses} border shadow-md px-5 py-4 my-4 rounded-md relative animate-slide-up flex`}
			role="alert"
		>
			<div className={`${iconClasses} mr-4 pt-1`}>
				<FontAwesomeIcon icon={icon} size="lg" />
			</div>
			<div>
				<div className="mb-1">
					<strong className="font-semibold text-white">{title}</strong>
				</div>
				<div>
					<span className="block sm:inline text-gray-200 opacity-90">
						{message}
					</span>
				</div>
			</div>
		</div>
	);
};

export default Alert;
