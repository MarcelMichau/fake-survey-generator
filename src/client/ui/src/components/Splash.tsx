const Splash = () => (
	<main className="my-6 grid gap-5 md:grid-cols-[1.2fr_1fr]">
		<section className="brutal-panel flex flex-col justify-center gap-6 py-10 sm:py-16">
			<p className="ui-label text-lime!">Create • Retrieve • Explore</p>
			<h1 className="display-title text-[clamp(3.5rem,8vw,7rem)]">
				Fake Survey Generator
			</h1>
			<p className="max-w-xl text-lg sm:text-xl">
				This is an app. That generates surveys. Fake ones. For fun. That is all.
			</p>
			<p className="border-l-[8px] border-lime bg-surface px-4 py-3 font-bold">
				Log In/Register to view/create surveys
			</p>
		</section>
		<div
			className="brutal-panel flex min-h-64 items-center justify-center bg-lime text-ink"
			aria-hidden="true"
		>
			<div className="w-full max-w-80 rotate-[-4deg] border-4 border-ink bg-paper p-6 shadow-[12px_12px_0_#101214]">
				<div className="display-title mb-6 text-4xl">The results are in.</div>
				<div className="mb-3 h-8 w-full bg-ink" />
				<div className="mb-3 h-8 w-4/5 bg-ink" />
				<div className="h-8 w-3/5 bg-ink" />
			</div>
		</div>
	</main>
);

export default Splash;
