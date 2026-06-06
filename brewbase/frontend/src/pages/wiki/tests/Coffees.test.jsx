import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import Coffees from "../Coffees";
import { sampleCoffee } from "../../../test/fixtures";

const navigateMock = vi.fn();

vi.mock("../../../api/coffeeApi", () => ({
    getCoffees: vi.fn(),
    addCoffeeFavorite: vi.fn(),
    removeCoffeeFavorite: vi.fn(),
}));

vi.mock("../../../api/articlesApi", () => ({
    getArticles: vi.fn(),
}));

vi.mock("../../../api/countryApi", () => ({
    getCountries: vi.fn(),
}));

vi.mock("../../../api/flavorProfileApi", () => ({
    getFlavorProfiles: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { getCoffees } from "../../../api/coffeeApi";
import { getArticles } from "../../../api/articlesApi";
import { getCountries } from "../../../api/countryApi";
import { getFlavorProfiles } from "../../../api/flavorProfileApi";

describe("Coffees", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getCoffees).mockReset();
        vi.mocked(getArticles).mockReset();
        vi.mocked(getCountries).mockReset();
        vi.mocked(getFlavorProfiles).mockReset();
    });

    it("shows loading state", () => {
        vi.mocked(getCoffees).mockImplementation(() => new Promise(() => {}));
        vi.mocked(getArticles).mockImplementation(() => new Promise(() => {}));
        vi.mocked(getCountries).mockImplementation(() => new Promise(() => {}));
        vi.mocked(getFlavorProfiles).mockImplementation(() => new Promise(() => {}));

        render(
            <MemoryRouter>
                <Coffees />
            </MemoryRouter>
        );

        expect(screen.getByText("Ładowanie kaw...")).toBeInTheDocument();
    });

    it("shows error when data cannot be loaded", async () => {
        vi.mocked(getCoffees).mockRejectedValue(new Error("fail"));

        render(
            <MemoryRouter>
                <Coffees />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Nie udało się pobrać kaw.")
        ).toBeInTheDocument();
    });

    it("shows empty state when there are no coffees", async () => {
        vi.mocked(getCoffees).mockResolvedValue([]);
        vi.mocked(getArticles).mockResolvedValue([]);
        vi.mocked(getCountries).mockResolvedValue([]);
        vi.mocked(getFlavorProfiles).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <Coffees />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Brak kaw do wyświetlenia.")
        ).toBeInTheDocument();
    });

    it("renders coffees and navigates to details", async () => {
        vi.mocked(getCoffees).mockResolvedValue([sampleCoffee]);
        vi.mocked(getArticles).mockResolvedValue([]);
        vi.mocked(getCountries).mockResolvedValue([]);
        vi.mocked(getFlavorProfiles).mockResolvedValue([]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Coffees />
            </MemoryRouter>
        );

        await screen.findByText("Etiopia Yirgacheffe");
        await user.click(screen.getByText("Etiopia Yirgacheffe"));

        expect(navigateMock).toHaveBeenCalledWith("/wiki/coffees/10");
    });

    it("filters coffees by search query", async () => {
        vi.mocked(getCoffees).mockResolvedValue([
            sampleCoffee,
            { ...sampleCoffee, id: 11, name: "Brazylia Santos" },
        ]);
        vi.mocked(getArticles).mockResolvedValue([]);
        vi.mocked(getCountries).mockResolvedValue([]);
        vi.mocked(getFlavorProfiles).mockResolvedValue([]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Coffees />
            </MemoryRouter>
        );

        await screen.findByText("Etiopia Yirgacheffe");

        await user.type(
            screen.getByPlaceholderText("Szukaj kaw..."),
            "Brazylia"
        );

        expect(screen.getByText("Brazylia Santos")).toBeInTheDocument();
        expect(screen.queryByText("Etiopia Yirgacheffe")).not.toBeInTheDocument();
    });
});
