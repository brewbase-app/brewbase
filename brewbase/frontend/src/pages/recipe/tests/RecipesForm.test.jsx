import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import RecipesForm from "../RecipesForm";
import { sampleRecipe } from "../../../test/fixtures";
import { ApiError } from "../../../api/apiClient";

const navigateMock = vi.fn();

vi.mock("../../../api/recipeApi", () => ({
    createRecipe: vi.fn(),
    getRecipeById: vi.fn(),
    updateRecipe: vi.fn(),
}));

vi.mock("../../../api/coffeeApi", () => ({
    getCoffees: vi.fn(),
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

import { createRecipe, getRecipeById, updateRecipe } from "../../../api/recipeApi";
import { getCoffees } from "../../../api/coffeeApi";
import { getBrewingMethods } from "../../../api/brewingMethodApi";

const catalogCoffees = [
    { id: 10, name: "Kenia AA", roastery: "Test", region: "Kenia" },
];

const catalogMethods = [
    { id: 1, name: "V60", description: "Pour over" },
];

function renderNewRecipeForm() {
    return render(
        <MemoryRouter initialEntries={["/recipes/new"]}>
            <Routes>
                <Route path="/recipes/new" element={<RecipesForm />} />
            </Routes>
        </MemoryRouter>
    );
}

function renderEditRecipeForm() {
    return render(
        <MemoryRouter initialEntries={["/recipes/edit/1"]}>
            <Routes>
                <Route path="/recipes/edit/:id" element={<RecipesForm />} />
            </Routes>
        </MemoryRouter>
    );
}

describe("RecipesForm", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(createRecipe).mockReset();
        vi.mocked(getRecipeById).mockReset();
        vi.mocked(updateRecipe).mockReset();
        vi.mocked(getCoffees).mockReset();
        vi.mocked(getBrewingMethods).mockReset();

        vi.mocked(getCoffees).mockResolvedValue(catalogCoffees);
        vi.mocked(getBrewingMethods).mockResolvedValue(catalogMethods);
    });

    it("renders new recipe form after catalog loads", async () => {
        renderNewRecipeForm();

        expect(await screen.findByText("Nowa receptura")).toBeInTheDocument();
        expect(screen.getByPlaceholderText("Nazwa receptury")).toBeInTheDocument();
    });

    it("shows catalog error with retry action", async () => {
        vi.mocked(getCoffees).mockRejectedValue(new Error("fail"));
        vi.mocked(getBrewingMethods).mockRejectedValue(new Error("fail"));

        const user = userEvent.setup();

        renderNewRecipeForm();

        expect(
            await screen.findByText(
                "Nie udało się pobrać katalogu kaw i metod parzenia."
            )
        ).toBeInTheDocument();

        vi.mocked(getCoffees).mockResolvedValue(catalogCoffees);
        vi.mocked(getBrewingMethods).mockResolvedValue(catalogMethods);

        await user.click(screen.getByRole("button", { name: "Spróbuj ponownie" }));

        await waitFor(() => {
            expect(getCoffees).toHaveBeenCalledTimes(2);
        });
    });

    it("shows validation error when saving empty draft", async () => {
        const user = userEvent.setup();

        renderNewRecipeForm();

        await screen.findByText("Nowa receptura");
        await user.click(
            screen.getByRole("button", { name: "Zapisz wersję roboczą" })
        );

        expect(
            screen.getByText(
                "Uzupełnij przynajmniej jedno pole, aby zapisać wersję roboczą."
            )
        ).toBeInTheDocument();
        expect(createRecipe).not.toHaveBeenCalled();
    });

    it("saves draft and navigates to my recipes", async () => {
        vi.mocked(createRecipe).mockResolvedValue({ id: 1 });

        const user = userEvent.setup();

        renderNewRecipeForm();

        await screen.findByText("Nowa receptura");

        await user.type(
            screen.getByPlaceholderText("Nazwa receptury"),
            "Poranna V60"
        );

        await user.click(
            screen.getByRole("button", { name: "Zapisz wersję roboczą" })
        );

        await waitFor(() => {
            expect(createRecipe).toHaveBeenCalled();
        });

        expect(navigateMock).toHaveBeenCalledWith("/recipes/my");
    });

    it("loads existing recipe in edit mode", async () => {
        vi.mocked(getRecipeById).mockResolvedValue({
            ...sampleRecipe,
            coffeeId: 10,
            brewingMethodId: 1,
            isPublic: false,
            moderationComment: "Uzupełnij opis.",
        });

        renderEditRecipeForm();

        expect(await screen.findByText("Edytuj recepturę")).toBeInTheDocument();
        expect(screen.getByDisplayValue("Poranna V60")).toBeInTheDocument();
        expect(screen.getByText("Uzupełnij opis.")).toBeInTheDocument();
    });

    it("maps backend validation errors to fields", async () => {
        const user = userEvent.setup();

        renderNewRecipeForm();

        await screen.findByText("Nowa receptura");

        await user.type(
            screen.getByPlaceholderText("Nazwa receptury"),
            "Poranna V60"
        );
        await user.type(
            screen.getByPlaceholderText("Opis przygotowania"),
            "Zalej kawę wodą i poczekaj."
        );

        vi.mocked(createRecipe).mockRejectedValue(
            new ApiError("Validation failed", 400, {
                title: ["Tytuł jest za krótki."],
            })
        );

        await user.click(
            screen.getByRole("button", { name: "Zapisz wersję roboczą" })
        );

        expect(
            await screen.findByText("Tytuł jest za krótki.")
        ).toBeInTheDocument();
    });
});
