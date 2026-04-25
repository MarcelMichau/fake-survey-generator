import { describe, it, expect, vi } from "vitest";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { render } from "../test/test-utils";
import ConfirmDialog from "./ConfirmDialog";

describe("ConfirmDialog", () => {
	it("does not render when closed", () => {
		render(
			<ConfirmDialog
				open={false}
				title="Delete survey?"
				message="Are you sure?"
				onConfirm={vi.fn()}
				onCancel={vi.fn()}
			/>,
		);

		expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
	});

	it("renders title and message when open", () => {
		render(
			<ConfirmDialog
				open
				title="Delete survey?"
				message="This cannot be undone."
				onConfirm={vi.fn()}
				onCancel={vi.fn()}
			/>,
		);

		expect(screen.getByRole("dialog")).toBeInTheDocument();
		expect(screen.getByText("Delete survey?")).toBeInTheDocument();
		expect(screen.getByText("This cannot be undone.")).toBeInTheDocument();
	});

	it("calls onConfirm when Confirm is clicked", async () => {
		const onConfirm = vi.fn();
		const user = userEvent.setup();

		render(
			<ConfirmDialog
				open
				title="Delete survey?"
				message="Are you sure?"
				confirmLabel="Delete"
				onConfirm={onConfirm}
				onCancel={vi.fn()}
			/>,
		);

		await user.click(screen.getByRole("button", { name: /^Delete$/ }));
		expect(onConfirm).toHaveBeenCalledTimes(1);
	});

	it("calls onCancel when Cancel is clicked", async () => {
		const onCancel = vi.fn();
		const user = userEvent.setup();

		render(
			<ConfirmDialog
				open
				title="Delete survey?"
				message="Are you sure?"
				onConfirm={vi.fn()}
				onCancel={onCancel}
			/>,
		);

		await user.click(screen.getByRole("button", { name: /Cancel/i }));
		expect(onCancel).toHaveBeenCalledTimes(1);
	});

	it("shows the busy label and ignores Escape while busy", async () => {
		const onCancel = vi.fn();
		const user = userEvent.setup();

		render(
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

		expect(
			screen.getByRole("button", { name: /Working\.\.\./ }),
		).toBeInTheDocument();

		await user.keyboard("{Escape}");
		expect(onCancel).not.toHaveBeenCalled();
	});

	it("calls onCancel when Escape is pressed and not busy", async () => {
		const onCancel = vi.fn();
		const user = userEvent.setup();

		render(
			<ConfirmDialog
				open
				title="Delete survey?"
				message="Are you sure?"
				onConfirm={vi.fn()}
				onCancel={onCancel}
			/>,
		);

		await user.keyboard("{Escape}");
		expect(onCancel).toHaveBeenCalledTimes(1);
	});
});
