import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import MyWikiArticleDetails from "../MyWikiArticleDetails";

vi.mock("../../../api/articlesApi", () => ({
    getMyArticleById: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useParams: () => ({ id: "1" }),
    };
});

import { getMyArticleById } from "../../../api/articlesApi";

const rejectedArticle = {
    id: 1,
    title: "Szkic o Kenii",
    module: "country",
    status: "Rejected",
    content: "Opis kraju pochodzenia kawy.",
    moderationComment: "Uzupełnij region i profile smakowe.",
};

function renderMyWikiArticleDetails() {
    return render(
        <MemoryRouter initialEntries={["/wiki/my-articles/1"]}>
            <Routes>
                <Route
                    path="/wiki/my-articles/:id"
                    element={<MyWikiArticleDetails />}
                />
            </Routes>
        </MemoryRouter>
    );
}

describe("MyWikiArticleDetails", () => {
    beforeEach(() => {
        vi.mocked(getMyArticleById).mockReset();
    });

    it("shows loading state initially", () => {
        vi.mocked(getMyArticleById).mockImplementation(() => new Promise(() => {}));

        renderMyWikiArticleDetails();

        expect(screen.getByText("Ładowanie...")).toBeInTheDocument();
    });

    it("shows error when article cannot be loaded", async () => {
        vi.mocked(getMyArticleById).mockRejectedValue(new Error("fail"));

        renderMyWikiArticleDetails();

        expect(
            await screen.findByText("Nie udało się pobrać artykułu.")
        ).toBeInTheDocument();
    });

    it("renders article details with module and status", async () => {
        vi.mocked(getMyArticleById).mockResolvedValue(rejectedArticle);

        renderMyWikiArticleDetails();

        expect(await screen.findByText("Szkic o Kenii")).toBeInTheDocument();
        expect(screen.getByText(/Kraje/)).toBeInTheDocument();
        expect(screen.getByText(/Odrzucony/)).toBeInTheDocument();
        expect(
            screen.getByText("Opis kraju pochodzenia kawy.")
        ).toBeInTheDocument();
    });

    it("shows moderation comment for rejected article", async () => {
        vi.mocked(getMyArticleById).mockResolvedValue(rejectedArticle);

        renderMyWikiArticleDetails();

        expect(
            await screen.findByText(
                /Komentarz moderatora: Uzupełnij region i profile smakowe./
            )
        ).toBeInTheDocument();
    });
});
