import { useState } from "react";
import AuthButton from "./AuthButton";
import VersionInfo from "./VersionInfo";

const NavBar = () => {
	const [collapsed, setCollapsed] = useState(true);

	return (
		<nav
			className="mt-3 flex flex-wrap items-center gap-4 border-2 border-white bg-lime px-4 py-4 text-ink shadow-[6px_6px_0_#050605] xl:flex-nowrap xl:px-5"
			aria-label="Main navigation"
		>
			<div className="display-title min-w-0 grow text-[clamp(2rem,3.3vw,3rem)] xl:grow-0 xl:shrink-0 xl:mr-2 xl:whitespace-nowrap">
				Fake Survey Generator
			</div>
			<button
				type="button"
				aria-expanded={!collapsed}
				aria-controls="nav-links"
				aria-label="Toggle menu"
				className="brutal-button brutal-button--secondary xl:hidden"
				onClick={() => setCollapsed(!collapsed)}
			>
				Menu
			</button>
			<div
				id="nav-links"
				className={`${collapsed ? "hidden" : "flex"} w-full flex-col gap-4 xl:flex xl:w-auto xl:grow xl:flex-row xl:items-center xl:justify-between`}
			>
				<div className="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:gap-x-5 xl:items-center">
					<div className="flex flex-wrap gap-x-5 gap-y-2 font-bold uppercase text-sm">
						<a className="hover:underline underline-offset-4" href="/api-docs">
							OpenAPI Docs
						</a>
						<a
							className="hover:underline underline-offset-4"
							href="/health/ready"
						>
							Health Check
						</a>
						<a
							className="hover:underline underline-offset-4"
							href="https://github.com/MarcelMichau/fake-survey-generator"
						>
							GitHub Repo
						</a>
					</div>
					<VersionInfo />
				</div>
				<AuthButton />
			</div>
		</nav>
	);
};

export default NavBar;
