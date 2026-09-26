import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCopyright } from "@fortawesome/free-solid-svg-icons";

const Footer = () => (
	<footer className="mx-auto flex w-full max-w-[1600px] flex-wrap items-center justify-between gap-3 px-4 py-6 text-sm text-paper sm:px-6 lg:px-8">
		<span>
			Marcel Michau <FontAwesomeIcon icon={faCopyright} className="mx-1" />{" "}
			{new Date().getFullYear()}
		</span>
	</footer>
);

export default Footer;
