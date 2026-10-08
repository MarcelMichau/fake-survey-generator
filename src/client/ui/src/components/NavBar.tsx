import { useState } from "react";
import AuthButton from "./AuthButton";

const navLinks = [
	{ label: "OpenAPI Docs", href: "/api-docs" },
	{ label: "Health Check", href: "/health/ready" },
	{
		label: "GitHub Repo",
		href: "https://github.com/MarcelMichau/fake-survey-generator",
	},
];

const NavBar = () => {
	const [collapsed, setCollapsed] = useState(true);

	return (
		<nav
			className="mt-4 flex flex-wrap items-center gap-4 border-2 border-paper bg-lime px-4 py-3 text-ink shadow-[6px_6px_0_var(--color-paper)] lg:flex-nowrap lg:px-5"
			aria-label="Main navigation"
		>
			<div className="display-title min-w-0 grow text-[clamp(1.9rem,3vw,2.75rem)] lg:grow-0 lg:shrink-0 lg:mr-2 lg:whitespace-nowrap">
				Fake Survey Generator
			</div>
			<button
				type="button"
				aria-expanded={!collapsed}
				aria-controls="nav-links"
				aria-label="Toggle menu"
				className="brutal-button brutal-button--secondary lg:hidden"
				onClick={() => setCollapsed(!collapsed)}
			>
				Menu
			</button>
			<div
				id="nav-links"
				className={`${collapsed ? "hidden" : "flex"} w-full flex-col gap-4 lg:flex lg:w-auto lg:grow lg:flex-row lg:items-center lg:justify-between`}
			>
				<div className="-ml-2 flex flex-col gap-1 text-sm font-bold uppercase tracking-wide sm:flex-row sm:flex-wrap lg:ml-0 lg:items-center">
					{navLinks.map(({ label, href }) => (
						<a
							key={href}
							className="px-2 py-1 transition-colors hover:bg-ink hover:text-lime"
							href={href}
						>
							{label}
						</a>
					))}
				</div>
				<AuthButton />
			</div>
		</nav>
	);
};

export default NavBar;
