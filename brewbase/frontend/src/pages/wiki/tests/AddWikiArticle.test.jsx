import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import AddWikiArticle from "../AddWikiArticle";

const navigateMock = vi.fn();

vi.mock("../../../api/articlesApi", () => ({
    createArticle: vi.fn(),
    getArticles: vi.fn(),
}));

vi.mock("../../../api/brewingMethodApi", () => ({
    getBrewingMethods: vi.fn(),
}));

vi.mock("../../../api/coffeeApi", () => ({
    lookupCoffeesByName: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { createArticle, getArticles } from "../../../api/articlesApi";
import { getBrewingMethods } from "../../../api/brewingMethodApi";

function renderAddWikiArticle(path = "/wiki/add") {
    return render(
        <MemoryRouter initialEntries={[path]}>
            <Routes>
                <Route path="/wiki/add" element={<AddWikiArticle />} />
            </Routes>
        </MemoryRouter>
    );
}

describe("AddWikiArticle", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(createArticle).mockReset();
        vi.mocked(getArticles).mockReset();
        vi.mocked(getBrewingMethods).mockReset();
        vi.mocked(getBrewingMethods).mockResolvedValue([
            { id: 1, name: "V60" },
        ]);
        vi.mocked(getArticles).mockResolvedValue([]);
    });

    it("renders add article form", () => {
        renderAddWikiArticle();

        expect(screen.getByText("Dodaj treść do wiki")).toBeInTheDocument();
        expect(screen.getByPlaceholderText("Wybierz kategorię")).toBeInTheDocument();
    });

    it("preselects category from module query param", async () => {
        renderAddWikiArticle("/wiki/add?module=brewing_method");

        expect(
            await screen.findByDisplayValue("Metody parzenia")
        ).toBeInTheDocument();
    });

    it("shows validation error when required fields are missing", async () => {
        const user = userEvent.setup();

        renderAddWikiArticle("/wiki/add?module=brewing_method");

        await screen.findByDisplayValue("Metody parzenia");
        await user.click(
            screen.getByRole("button", { name: /Wyślij do moderacji/i })
        );

        expect(
            screen.getByText(
                "Uzupełnij wymagane pola: metodę parzenia, opis i kategorię."
            )
        ).toBeInTheDocument();
        expect(createArticle).not.toHaveBeenCalled();
    });

    it("submits brewing article and navigates to wiki home", async () => {
        vi.mocked(createArticle).mockResolvedValue({ id: 12 });

        const user = userEvent.setup();

        renderAddWikiArticle("/wiki/add?module=brewing_method");

        await screen.findByDisplayValue("Metody parzenia");

        const methodSelect = await screen.findByRole("combobox");
        await user.selectOptions(methodSelect, "V60");

        await user.type(
            screen.getByPlaceholderText("Dodaj opis metody..."),
            "Opis metody V60."
        );

        await user.click(
            screen.getByRole("button", { name: /Wyślij do moderacji/i })
        );

        await waitFor(() => {
            expect(createArticle).toHaveBeenCalledWith({
                title: "V60",
                content: "Opis metody V60.",
                module: "brewing_method",
                coffeeId: undefined,
            });
        });

        expect(navigateMock).toHaveBeenCalledWith("/wiki", {
            state: { articleSubmitted: true },
        });
    });
});
