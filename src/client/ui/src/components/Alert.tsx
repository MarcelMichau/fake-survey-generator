import {
	faCircleCheck,
	faCircleExclamation,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

type AlertType = "error" | "success" | "warning";
type AlertProps = { title: string; message: string; type?: AlertType };

const Alert = ({ title, message, type = "success" }: AlertProps) => {
	const icon = type === "success" ? faCircleCheck : faCircleExclamation;
	const accents = {
		success: "border-lime bg-[#263923]",
		error: "border-[#ff8e83] bg-[#3b2525]",
		warning: "border-[#ffdc75] bg-[#383322]",
	};
	return (
		<div
			className={`my-4 flex gap-4 border-2 p-4 ${accents[type]}`}
			role="alert"
		>
			<FontAwesomeIcon
				icon={icon}
				className={`mt-1 shrink-0 text-xl ${type === "success" ? "text-lime" : type === "error" ? "text-[#ffaaa1]" : "text-[#ffdc75]"}`}
			/>
			<div className="min-w-0">
				<strong className="ui-label mb-1">{title}</strong>
				<p className="text-paper wrap-break-word">{message}</p>
			</div>
		</div>
	);
};

export default Alert;
