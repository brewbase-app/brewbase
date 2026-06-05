import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import CoffeeDetails from "../CoffeeDetails";
import { sampleCoffee, sampleProfile } from "../../../test/fixtures";

vi.mock("../../../api/coffeeApi", () => ({
    getCoffeeById: vi.fn(),
    rateCoffee: vi.fn(),
    addCoffeeFavorite: vi.fn(),
    removeCoffeeFavorite: vi.fn(),
}));

vi.mock("../../../api/profileApi", () => ({
    getProfile: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useParams: () => ({ id: "10" }),
        useNavigate: () => vi.fn(),
    };
});

import { getCoffeeById, rateCoffee } from "../../../api/coffeeApi";
import { getProfile } from "../../../api/profileApi";

function renderCoffeeDetails() {
    return render(
        <MemoryRouter initialEntries={["/wiki/coffees/10"]}>
            <Routes>
                <Route path="/wiki/coffees/:id" element={<CoffeeDetails />} />
            </Routes>
        </MemoryRouter>
    );
}

describe("CoffeeDetails", () => {
    beforeEach(() => {
        vi.mocked(getCoffeeById).mockReset();
        vi.mocked(rateCoffee).mockReset();
        vi.mocked(getProfile).mockReset();
    });

    it("shows loading state initially", () => {
        vi.mocked(getCoffeeById).mockImplementation(() => new Promise(() => {}));
        vi.mocked(getProfile).mockResolvedValue(sampleProfile);

        renderCoffeeDetails();

        expect(screen.getByText("Ładowanie...")).toBeInTheDocument();
    });

    it("shows message when user cannot rate own coffee", async () => {
        vi.mocked(getCoffeeById).mockResolvedValue({
            ...sampleCoffee,
            createdByUserId: 1,
        });
        vi.mocked(getProfile).mockResolvedValue({
            ...sampleProfile,
            userId: 1,
        });

        renderCoffeeDetails();

        expect(
            await screen.findByText("Nie możesz oceniać własnej kawy")
        ).toBeInTheDocument();
    });

    it("submits rating for another users coffee", async () => {
        vi.mocked(getCoffeeById)
            .mockResolvedValueOnce(sampleCoffee)
            .mockResolvedValueOnce({
                ...sampleCoffee,
                averageRating: 4.6,
                ratingCount: 4,
            });
        vi.mocked(getProfile).mockResolvedValue(sampleProfile);
        vi.mocked(rateCoffee).mockResolvedValue(null);

        const user = userEvent.setup();

        renderCoffeeDetails();

        await screen.findByText("Etiopia Yirgacheffe");

        const starButtons = screen
            .getAllByRole("button")
            .filter((button) => button.className.includes("star-button"));

        await user.click(starButtons[3]);
        await user.click(screen.getByRole("button", { name: "Oceń" }));

        await waitFor(() => {
            expect(rateCoffee).toHaveBeenCalledWith("10", 4);
        });

        expect(
            await screen.findByText("Dziękujemy za ocenę!")
        ).toBeInTheDocument();
    });
});
