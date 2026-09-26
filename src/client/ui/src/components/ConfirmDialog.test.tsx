import { describe, expect, it, vi } from "vitest";
import { userEvent } from "vitest/browser";
import { render } from "../test/test-utils";
import ConfirmDialog from "./ConfirmDialog";

describe("ConfirmDialog", () => {
	it("does not render when closed", async () => {
		const screen = await render(
			<ConfirmDialog
				open={false}
				title="Delete survey?"
				message="Are you sure?"
				onConfirm={vi.fn()}
				onCancel={vi.fn()}
			/>,
		);

		await expect.element(screen.getByRole("dialog")).not.toBeInTheDocument();
	});

	it("renders title and message when open", async () => {
		const screen = await render(
			<ConfirmDialog
				open
				title="Delete survey?"
				message="This cannot be undone."
				onConfirm={vi.fn()}
				onCancel={vi.fn()}
			/>,
		);

		await expect.element(screen.getByRole("dialog")).toBeInTheDocument();
		await expect
			.element(screen.getByText("Delete survey?"))
			.toBeInTheDocument();
		await expect
			.element(screen.getByText("This cannot be undone."))
			.toBeInTheDocument();
	});

	it("calls onConfirm when Confirm is clicked", async () => {
		const onConfirm = vi.fn();
		const screen = await render(
			<ConfirmDialog
				open
				title="Delete survey?"
				message="Are you sure?"
				confirmLabel="Delete"
				onConfirm={onConfirm}
				onCancel={vi.fn()}
			/>,
		);

		await screen.getByRole("button", { name: /^Delete$/ }).click();
		expect(onConfirm).toHaveBeenCalledTimes(1);
	});

	it("calls onCancel when Cancel is clicked", async () => {
		const onCancel = vi.fn();
		const screen = await render(
			<ConfirmDialog
				open
				title="Delete survey?"
				message="Are you sure?"
				onConfirm={vi.fn()}
				onCancel={onCancel}
			/>,
		);

		await screen.getByRole("button", { name: /Cancel/i }).click();
		expect(onCancel).toHaveBeenCalledTimes(1);
	});

	it("shows the busy label and ignores Escape while busy", async () => {
		const onCancel = vi.fn();
		const screen = await render(
			<ConfirmDialog
				open
				title="Delete survey?"
				message="Are you sure you want to proceed?"
				confirmLabel="Delete"
				busy
				onConfirm={vi.fn()}
				onCancel={onCancel}
			/>,
		);

		await expect
			.element(screen.getByRole("button", { name: /Working\.\.\./ }))
			.toBeInTheDocument();

		await userEvent.keyboard("{Escape}");
		expect(onCancel).not.toHaveBeenCalled();
	});

	it("calls onCancel when Escape is pressed and not busy", async () => {
		const onCancel = vi.fn();
		await render(
			<ConfirmDialog
				open
				title="Delete survey?"
				message="Are you sure?"
				onConfirm={vi.fn()}
				onCancel={onCancel}
			/>,
		);

		await userEvent.keyboard("{Escape}");
		expect(onCancel).toHaveBeenCalledTimes(1);
	});

	it("restores the connected trigger on close", async () => {
		const dialog = (open: boolean) => (
			<>
				<button type="button" id="trigger">
					Delete
				</button>
				<button type="button" id="fallback">
					Get My Surveys
				</button>
				<ConfirmDialog
					open={open}
					title="Delete survey?"
					message="Sure?"
					fallbackFocus={() => document.getElementById("fallback")}
					onConfirm={vi.fn()}
					onCancel={vi.fn()}
				/>
			</>
		);
		const screen = await render(dialog(false));
		await screen.getByRole("button", { name: "Delete" }).click();
		await screen.rerender(dialog(true));
		await expect
			.element(screen.getByRole("button", { name: "Cancel" }))
			.toHaveFocus();
		await screen.rerender(dialog(false));
		await expect
			.element(screen.getByRole("button", { name: "Delete" }))
			.toHaveFocus();
	});

	it("focuses a surviving workflow control when the delete trigger is removed", async () => {
		const dialog = (open: boolean, showTrigger: boolean) => (
			<>
				{showTrigger && (
					<button type="button" id="trigger">
						Delete
					</button>
				)}
				<button type="button" id="fallback">
					Get My Surveys
				</button>
				<ConfirmDialog
					open={open}
					title="Delete survey?"
					message="Sure?"
					fallbackFocus={() => document.getElementById("fallback")}
					onConfirm={vi.fn()}
					onCancel={vi.fn()}
				/>
			</>
		);
		const screen = await render(dialog(false, true));
		await screen.getByRole("button", { name: "Delete" }).click();
		await screen.rerender(dialog(true, true));
		await screen.rerender(dialog(true, false));
		await screen.rerender(dialog(false, false));
		await expect
			.element(screen.getByRole("button", { name: "Get My Surveys" }))
			.toHaveFocus();
	});

	it("focuses the panel while busy and Cancel again when busy ends", async () => {
		const dialog = (busy: boolean) => (
			<>
				<button type="button" id="outside">
					Outside
				</button>
				<ConfirmDialog
					open
					busy={busy}
					title="Delete survey?"
					message="Sure?"
					onConfirm={vi.fn()}
					onCancel={vi.fn()}
				/>
			</>
		);
		const screen = await render(dialog(false));
		await expect
			.element(screen.getByRole("button", { name: "Cancel" }))
			.toHaveFocus();
		await screen.rerender(dialog(true));
		expect(document.activeElement).toBe(
			screen.container.querySelector(".brutal-panel"),
		);
		await screen.rerender(dialog(false));
		await expect
			.element(screen.getByRole("button", { name: "Cancel" }))
			.toHaveFocus();
		await screen.rerender(dialog(true));
		screen.container.querySelector<HTMLElement>("#outside")?.focus();
		await screen.rerender(dialog(false));
		await expect
			.element(screen.getByRole("button", { name: "Cancel" }))
			.toHaveFocus();
	});
});
