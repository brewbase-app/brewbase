import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import WikiHome from "../WikiHome";

const navigateMock = vi.fn();

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

function renderWikiHome(initialEntry = { pathname: "/wiki" }) {
    return render(
        <MemoryRouter initialEntries={[initialEntry]}>
            <Routes>
                <Route path="/wiki" element={<WikiHome />} />
            </Routes>
        </MemoryRouter>
    );
}

describe("WikiHome", () => {
    beforeEach(() => {
        navigateMock.mockReset();
    });

    it("renders wiki home header and categories", () => {
        renderWikiHome();

        expect(screen.getByText("Encyklopedia Kawy")).toBeInTheDocument();
        expect(screen.getByText("Kawy")).toBeInTheDocument();
        expect(screen.getByText("Kraje")).toBeInTheDocument();
        expect(screen.getByText("Metody Parzenia")).toBeInTheDocument();
        expect(screen.getByText("Palarnie")).toBeInTheDocument();
    });

    it("navigates to category pages", async () => {
        const user = userEvent.setup();

        renderWikiHome();

        await user.click(screen.getByText("Kawy"));
        expect(navigateMock).toHaveBeenCalledWith("/wiki/coffees");

        await user.click(screen.getByText("Palarnie"));
        expect(navigateMock).toHaveBeenCalledWith("/wiki/roasteries");
    });

    it("navigates to my articles and add article", async () => {
        const user = userEvent.setup();

        renderWikiHome();

        await user.click(screen.getByRole("button", { name: /Moje artykuły/i }));
        expect(navigateMock).toHaveBeenCalledWith("/wiki/my-articles");

        await user.click(screen.getByRole("button", { name: /Nowy artykuł/i }));
        expect(navigateMock).toHaveBeenCalledWith("/wiki/add");
    });

    it("shows submit success message from navigation state", async () => {
        const user = userEvent.setup();

        renderWikiHome({
            pathname: "/wiki",
            state: { articleSubmitted: true },
        });

        expect(
            await screen.findByText(/Artykuł został wysłany do moderacji/)
        ).toBeInTheDocument();

        await user.click(
            screen.getByRole("button", { name: "Zamknij komunikat" })
        );

        expect(
            screen.queryByText(/Artykuł został wysłany do moderacji/)
        ).not.toBeInTheDocument();
    });
});
