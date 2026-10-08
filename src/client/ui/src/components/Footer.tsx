import { faCopyright } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import VersionInfo from "./VersionInfo";

const Footer = () => (
	<footer className="mx-auto w-full max-w-[1600px] px-4 sm:px-6 lg:px-8">
		<div className="flex flex-wrap items-center justify-between gap-x-6 gap-y-3 border-t-2 border-line py-5 text-sm text-muted">
			<span>
				Marcel Michau <FontAwesomeIcon icon={faCopyright} className="mx-1" />{" "}
				{new Date().getFullYear()}
			</span>
			<div className="flex flex-wrap gap-2">
				<VersionInfo />
			</div>
		</div>
	</footer>
);

export default Footer;
