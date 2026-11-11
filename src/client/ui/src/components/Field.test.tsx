import { useState } from "react";
import { describe, expect, it } from "vitest";
import { render } from "vitest-browser-react";
import "../index.css";
import { AnnouncementProvider } from "./Announcements";
import Field from "./Field";

function Form() {
	const [value, setValue] = useState("");
	return (
		<AnnouncementProvider>
			<form onSubmit={(event) => event.preventDefault()}>
				<Field
					label="Digits"
					value={value}
					onChange={setValue}
					loading={false}
					required
					pattern="[0-9]+"
					hint="Use digits only."
				/>
				<label htmlFor="other">Other</label>
				<input id="other" />
				<button type="submit">Save</button>
			</form>
		</AnnouncementProvider>
	);
}

describe("Field validation feedback", () => {
	it("waits for interaction, announces a committed error politely, and clears corrected feedback", async () => {
		const screen = await render(<Form />);
		const input = screen.getByRole("textbox", { name: "Digits" });
		await expect.element(input).toHaveAttribute("aria-invalid", "false");
		await expect.element(input).toHaveAccessibleDescription("Use digits only.");
		await input.fill("abc");
		await expect.element(input).toHaveAttribute("aria-invalid", "false");
		await screen.getByRole("textbox", { name: "Other" }).fill("x");
		await expect.element(input).toHaveAttribute("aria-invalid", "true");
		await expect
			.poll(
				() => screen.container.querySelector('[role="status"]')?.textContent,
			)
			.toMatch(/^Digits: .+/);
		await input.fill("123");
		await expect.element(input).toHaveAttribute("aria-invalid", "false");
		await expect.element(input).toHaveAccessibleDescription("Use digits only.");
		await expect.element(screen.getByRole("status")).toBeEmptyDOMElement();
	});
});
