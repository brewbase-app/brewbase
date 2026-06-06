import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import BrewingMethods from "../BrewingMethods";

const navigateMock = vi.fn();

vi.mock("../../../api/articlesApi", () => ({
    getArticles: vi.fn(),
}));

vi.mock("../../../api/brewingMethodApi", () => ({
    getBrewingMethods: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { getArticles } from "../../../api/articlesApi";
import { getBrewingMethods } from "../../../api/brewingMethodApi";

const brewingArticle = {
    id: 12,
    title: "V60",
    content: "Metoda pour-over oparta o papierowy filtr.",
};

describe("BrewingMethods", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getArticles).mockReset();
        vi.mocked(getBrewingMethods).mockReset();
    });

    it("shows loading state", () => {
        vi.mocked(getArticles).mockImplementation(() => new Promise(() => {}));
        vi.mocked(getBrewingMethods).mockImplementation(() => new Promise(() => {}));

        render(
            <MemoryRouter>
                <BrewingMethods />
            </MemoryRouter>
        );

        expect(screen.getByText("Ładowanie metod parzenia...")).toBeInTheDocument();
    });

    it("shows error when data cannot be loaded", async () => {
        vi.mocked(getArticles).mockRejectedValue(new Error("fail"));

        render(
            <MemoryRouter>
                <BrewingMethods />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Nie udało się pobrać metod parzenia.")
        ).toBeInTheDocument();
    });

    it("shows empty state when there are no articles", async () => {
        vi.mocked(getArticles).mockResolvedValue([]);
        vi.mocked(getBrewingMethods).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <BrewingMethods />
            </MemoryRouter>
        );

        expect(
            await screen.findByText(
                "Brak opublikowanych artykułów o metodach parzenia."
            )
        ).toBeInTheDocument();
    });

    it("renders brewing method articles and navigates on click", async () => {
        vi.mocked(getArticles).mockResolvedValue([brewingArticle]);
        vi.mocked(getBrewingMethods).mockResolvedValue([
            { id: 1, name: "V60" },
        ]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <BrewingMethods />
            </MemoryRouter>
        );

        await screen.findByRole("heading", { name: "V60" });
        await user.click(screen.getByRole("heading", { name: "V60" }));

        expect(navigateMock).toHaveBeenCalledWith("/wiki/articles/12");
    });

    it("filters methods by search query", async () => {
        vi.mocked(getArticles).mockResolvedValue([
            brewingArticle,
            { ...brewingArticle, id: 13, title: "AeroPress", content: "Szybka metoda." },
        ]);
        vi.mocked(getBrewingMethods).mockResolvedValue([
            { id: 1, name: "V60" },
            { id: 2, name: "AeroPress" },
        ]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <BrewingMethods />
            </MemoryRouter>
        );

        await screen.findByRole("heading", { name: "V60" });

        await user.type(
            screen.getByPlaceholderText("Szukaj metod..."),
            "AeroPress"
        );

        expect(screen.getByRole("heading", { name: "AeroPress" })).toBeInTheDocument();
        expect(screen.queryByRole("heading", { name: "V60" })).not.toBeInTheDocument();
    });
});
