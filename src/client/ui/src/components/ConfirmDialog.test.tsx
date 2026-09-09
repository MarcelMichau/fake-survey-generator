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
});
