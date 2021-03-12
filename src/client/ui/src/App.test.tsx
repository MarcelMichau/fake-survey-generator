import { render, screen } from "@testing-library/react";
import App from "./App";

test("Renders Fake Survey Generator Title", () => {
    render(<App />);
    const linkElement = screen.getByText(/Fake Survey Generator/i);
    expect(linkElement).toBeInTheDocument();
});
