import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import RecipesList from "../RecipesList";

const navigateMock = vi.fn();

vi.mock("../../../api/recipeApi", () => ({
    getRecipes: vi.fn(),
    getMyRecipes: vi.fn(),
    getFavoriteRecipes: vi.fn(),
    deleteRecipe: vi.fn(),
    addFavorite: vi.fn(),
    removeFavorite: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import {
    addFavorite,
    deleteRecipe,
    getFavoriteRecipes,
    getMyRecipes,
    getRecipes,
    removeFavorite,
} from "../../../api/recipeApi";

const publicRecipe = {
    id: 1,
    title: "Poranna V60",
    createdAt: "2026-01-15T10:00:00Z",
    isPublic: true,
    isFavorite: false,
};

const privateRecipe = {
    id: 2,
    title: "Robocza AeroPress",
    createdAt: "2026-01-16T10:00:00Z",
    isPublic: false,
    isFavorite: false,
    moderationComment: "Uzupełnij opis parzenia.",
};

const favoriteRecipe = {
    id: 3,
    title: "Ulubiona Chemex",
    createdAt: "2026-01-17T10:00:00Z",
    isPublic: true,
    isFavorite: true,
};

describe("RecipesList", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getRecipes).mockReset();
        vi.mocked(getMyRecipes).mockReset();
        vi.mocked(getFavoriteRecipes).mockReset();
        vi.mocked(deleteRecipe).mockReset();
        vi.mocked(addFavorite).mockReset();
        vi.mocked(removeFavorite).mockReset();
        vi.spyOn(window, "confirm").mockReturnValue(true);
        vi.spyOn(window, "alert").mockImplementation(() => {});
    });

    it("shows empty state for all recipes", async () => {
        vi.mocked(getRecipes).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <RecipesList title="Wszystkie receptury" />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Brak receptur.")
        ).toBeInTheDocument();
    });

    it("renders only public recipes in all recipes view", async () => {
        vi.mocked(getRecipes).mockResolvedValue([publicRecipe, privateRecipe]);

        render(
            <MemoryRouter>
                <RecipesList title="Wszystkie receptury" />
            </MemoryRouter>
        );

        expect(await screen.findByText("Poranna V60")).toBeInTheDocument();
        expect(screen.queryByText("Robocza AeroPress")).not.toBeInTheDocument();
    });

    it("renders my recipes with moderation comment and delete action", async () => {
        vi.mocked(getMyRecipes).mockResolvedValue([privateRecipe]);

        render(
            <MemoryRouter>
                <RecipesList title="Twoje receptury" />
            </MemoryRouter>
        );

        expect(await screen.findByText("Robocza AeroPress")).toBeInTheDocument();
        expect(
            screen.getByText(/Komentarz moderatora: Uzupełnij opis parzenia./)
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: /Usuń/i })
        ).toBeInTheDocument();
    });

    it("renders favorite recipes without delete action", async () => {
        vi.mocked(getFavoriteRecipes).mockResolvedValue([favoriteRecipe]);

        render(
            <MemoryRouter>
                <RecipesList title="Ulubione receptury" />
            </MemoryRouter>
        );

        expect(await screen.findByText("Ulubiona Chemex")).toBeInTheDocument();
        expect(
            document.querySelector(".recipe-list__btn--danger")
        ).not.toBeInTheDocument();
    });

    it("toggles favorite state", async () => {
        vi.mocked(getRecipes).mockResolvedValue([publicRecipe]);
        vi.mocked(addFavorite).mockResolvedValue({});

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <RecipesList title="Wszystkie receptury" />
            </MemoryRouter>
        );

        await screen.findByText("Poranna V60");
        await user.click(
            screen.getByRole("button", { name: "Dodaj do ulubionych" })
        );

        await waitFor(() => {
            expect(addFavorite).toHaveBeenCalledWith(1);
        });
    });

    it("deletes my recipe after confirmation", async () => {
        vi.mocked(getMyRecipes).mockResolvedValue([privateRecipe]);
        vi.mocked(deleteRecipe).mockResolvedValue({});

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <RecipesList title="Twoje receptury" />
            </MemoryRouter>
        );

        await screen.findByText("Robocza AeroPress");
        await user.click(screen.getByRole("button", { name: /Usuń/i }));

        await waitFor(() => {
            expect(deleteRecipe).toHaveBeenCalledWith(2);
        });

        expect(screen.queryByText("Robocza AeroPress")).not.toBeInTheDocument();
    });

    it("navigates to recipe details", async () => {
        vi.mocked(getRecipes).mockResolvedValue([publicRecipe]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <RecipesList title="Wszystkie receptury" />
            </MemoryRouter>
        );

        await screen.findByText("Poranna V60");
        await user.click(screen.getByRole("button", { name: /Szczegóły/i }));

        expect(navigateMock).toHaveBeenCalledWith("/recipes/1");
    });
});
