import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import RecipeDetails from "../RecipeDetails";
import { sampleProfile, sampleRecipe } from "../../../test/fixtures";
import { ApiError } from "../../../api/apiClient";

vi.mock("../../../api/recipeApi", () => ({
    getRecipeById: vi.fn(),
    rateRecipe: vi.fn(),
    addFavorite: vi.fn(),
    removeFavorite: vi.fn(),
}));

vi.mock("../../../api/profileApi", () => ({
    getProfile: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useParams: () => ({ id: "1" }),
        useNavigate: () => vi.fn(),
    };
});

import {
    getRecipeById,
    rateRecipe,
} from "../../../api/recipeApi";
import { getProfile } from "../../../api/profileApi";

function renderRecipeDetails() {
    return render(
        <MemoryRouter initialEntries={["/recipes/1"]}>
            <Routes>
                <Route path="/recipes/:id" element={<RecipeDetails />} />
            </Routes>
        </MemoryRouter>
    );
}

async function clickRatingStar(starIndex) {
    const ratingSummary = screen.getByText(/Brak ocen|\d\.\d/);
    const starsContainer = ratingSummary.previousElementSibling;

    expect(starsContainer).toBeTruthy();

    const stars = starsContainer.querySelectorAll("svg");
    await userEvent.setup().click(stars[starIndex]);
}

describe("RecipeDetails", () => {
    beforeEach(() => {
        vi.mocked(getRecipeById).mockReset();
        vi.mocked(rateRecipe).mockReset();
        vi.mocked(getProfile).mockReset();
        vi.spyOn(window, "alert").mockImplementation(() => {});
    });

    it("shows loading state initially", () => {
        vi.mocked(getRecipeById).mockImplementation(() => new Promise(() => {}));
        vi.mocked(getProfile).mockResolvedValue(sampleProfile);

        renderRecipeDetails();

        expect(screen.getByText("Ładowanie receptury...")).toBeInTheDocument();
    });

    it("shows message when user cannot rate own recipe", async () => {
        vi.mocked(getRecipeById).mockResolvedValue({
            ...sampleRecipe,
            userId: 1,
        });
        vi.mocked(getProfile).mockResolvedValue({
            ...sampleProfile,
            userId: 1,
        });

        renderRecipeDetails();

        expect(
            await screen.findByText("Nie możesz oceniać własnej receptury")
        ).toBeInTheDocument();
        expect(screen.queryByText("Poranna V60")).toBeInTheDocument();
    });

    it("rates another users recipe and refreshes average", async () => {
        let recipeFetchCount = 0;

        vi.mocked(getRecipeById).mockImplementation(async () => {
            recipeFetchCount += 1;

            if (recipeFetchCount === 1) {
                return sampleRecipe;
            }

            return {
                ...sampleRecipe,
                averageRating: 4,
                ratingCount: 1,
                userRating: 4,
            };
        });
        vi.mocked(getProfile).mockResolvedValue(sampleProfile);
        vi.mocked(rateRecipe).mockResolvedValue(null);

        renderRecipeDetails();

        await screen.findByText("Poranna V60");
        await clickRatingStar(3);

        await waitFor(() => {
            expect(rateRecipe).toHaveBeenCalledWith(1, 4);
        });

        await waitFor(() => {
            expect(screen.getByText(/4\.0/)).toBeInTheDocument();
            expect(screen.getByText(/1 ocen/)).toBeInTheDocument();
            expect(
                screen.getByText("Twoja ocena została zapisana.")
            ).toBeInTheDocument();
            expect(screen.getByText("Twoja ocena: 4/5")).toBeInTheDocument();
        });
    });

    it("shows saved rating and blocks rating again when user already rated", async () => {
        vi.mocked(getRecipeById).mockResolvedValue({
            ...sampleRecipe,
            userRating: 3,
            averageRating: 4.2,
            ratingCount: 5,
        });
        vi.mocked(getProfile).mockResolvedValue(sampleProfile);

        renderRecipeDetails();

        expect(
            await screen.findByText("Twoja ocena: 3/5")
        ).toBeInTheDocument();

        await clickRatingStar(4);

        expect(rateRecipe).not.toHaveBeenCalled();
    });

    it("shows alert when rating fails with forbidden", async () => {
        vi.mocked(getRecipeById).mockResolvedValue(sampleRecipe);
        vi.mocked(getProfile).mockResolvedValue(sampleProfile);
        vi.mocked(rateRecipe).mockRejectedValue(
            new ApiError("Forbidden", 403)
        );

        renderRecipeDetails();

        await screen.findByText("Poranna V60");
        await clickRatingStar(2);

        await waitFor(() => {
            expect(window.alert).toHaveBeenCalledWith(
                "Nie możesz oceniać własnej receptury."
            );
        });
    });
});
