import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import Roasteries from "../Roasteries";

const navigateMock = vi.fn();

vi.mock("../../../api/articlesApi", () => ({
    getArticles: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { getArticles } from "../../../api/articlesApi";

const roasteryArticle = {
    id: 5,
    title: "Hola Coffee",
    authorLogin: "maria",
    content: "Styl palenia: jasne palenie\n\nOpis palarni specialty.",
};

describe("Roasteries", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getArticles).mockReset();
    });

    it("shows loading state", () => {
        vi.mocked(getArticles).mockImplementation(() => new Promise(() => {}));

        render(
            <MemoryRouter>
                <Roasteries />
            </MemoryRouter>
        );

        expect(screen.getByText("Ładowanie palarni...")).toBeInTheDocument();
    });

    it("shows error when articles cannot be loaded", async () => {
        vi.mocked(getArticles).mockRejectedValue(new Error("fail"));

        render(
            <MemoryRouter>
                <Roasteries />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Nie udało się pobrać artykułów o palarniach.")
        ).toBeInTheDocument();
    });

    it("shows empty state when there are no articles", async () => {
        vi.mocked(getArticles).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <Roasteries />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Brak zatwierdzonych artykułów o palarniach.")
        ).toBeInTheDocument();
    });

    it("renders roastery articles and navigates on click", async () => {
        vi.mocked(getArticles).mockResolvedValue([roasteryArticle]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Roasteries />
            </MemoryRouter>
        );

        await screen.findByText("Hola Coffee");
        await user.click(screen.getByText("Hola Coffee"));

        expect(navigateMock).toHaveBeenCalledWith("/wiki/articles/5");
    });

    it("filters roasteries by search query", async () => {
        vi.mocked(getArticles).mockResolvedValue([
            roasteryArticle,
            { ...roasteryArticle, id: 6, title: "Coffee Proficiency" },
        ]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Roasteries />
            </MemoryRouter>
        );

        await screen.findByText("Hola Coffee");

        await user.type(
            screen.getByPlaceholderText("Szukaj palarni..."),
            "Proficiency"
        );

        expect(screen.getByText("Coffee Proficiency")).toBeInTheDocument();
        expect(screen.queryByText("Hola Coffee")).not.toBeInTheDocument();
    });
});
