import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import WikiArticleDetails from "../WikiArticleDetails";

const navigateMock = vi.fn();

vi.mock("../../../api/articlesApi", () => ({
    getArticleById: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { getArticleById } from "../../../api/articlesApi";

const roasteryArticle = {
    id: 5,
    title: "Hola Coffee",
    authorLogin: "maria",
    module: "roastery",
    content: "Styl palenia: jasne palenie\n\nOpis palarni specialty.",
};

const coffeeArticle = {
    id: 7,
    title: "Kenia AA",
    authorLogin: "jan",
    module: "coffee",
    status: "Approved",
    coffeeId: 42,
    content: "Opis kawy.",
};

function CoffeeDetailsPage() {
    return <div>Szczegóły kawy</div>;
}

function renderWikiArticleDetails() {
    return render(
        <MemoryRouter initialEntries={["/wiki/articles/5"]}>
            <Routes>
                <Route path="/wiki/articles/:id" element={<WikiArticleDetails />} />
                <Route path="/wiki/coffees/:id" element={<CoffeeDetailsPage />} />
            </Routes>
        </MemoryRouter>
    );
}

describe("WikiArticleDetails", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getArticleById).mockReset();
    });

    it("shows loading state initially", () => {
        vi.mocked(getArticleById).mockImplementation(() => new Promise(() => {}));

        renderWikiArticleDetails();

        expect(screen.getByText("Ładowanie...")).toBeInTheDocument();
    });

    it("shows error when article cannot be loaded", async () => {
        vi.mocked(getArticleById).mockRejectedValue(new Error("fail"));

        renderWikiArticleDetails();

        expect(
            await screen.findByText("Nie udało się pobrać artykułu.")
        ).toBeInTheDocument();
    });

    it("renders roastery article details", async () => {
        vi.mocked(getArticleById).mockResolvedValue(roasteryArticle);

        renderWikiArticleDetails();

        expect(await screen.findByText("Hola Coffee")).toBeInTheDocument();
        expect(screen.getByText(/Autor artykułu: maria/)).toBeInTheDocument();
        expect(screen.getByText("Opis palarni specialty.")).toBeInTheDocument();
    });

    it("redirects approved coffee article to coffee details route", async () => {
        vi.mocked(getArticleById).mockResolvedValue(coffeeArticle);

        render(
            <MemoryRouter initialEntries={["/wiki/articles/7"]}>
                <Routes>
                    <Route path="/wiki/articles/:id" element={<WikiArticleDetails />} />
                    <Route path="/wiki/coffees/:id" element={<CoffeeDetailsPage />} />
                </Routes>
            </MemoryRouter>
        );

        await waitFor(() => {
            expect(navigateMock).toHaveBeenCalledWith("/wiki/coffees/42", {
                replace: true,
            });
        });
    });

    it("navigates to report page", async () => {
        vi.mocked(getArticleById).mockResolvedValue(roasteryArticle);

        const user = userEvent.setup();

        renderWikiArticleDetails();

        await screen.findByText("Hola Coffee");
        await user.click(screen.getByRole("button", { name: /Zgłoś treść/i }));

        expect(navigateMock).toHaveBeenCalledWith("/report", {
            state: {
                contentType: "article",
                contentId: 5,
                contentTitle: "Hola Coffee",
                returnPath: "/wiki/articles/5",
            },
        });
    });
});
