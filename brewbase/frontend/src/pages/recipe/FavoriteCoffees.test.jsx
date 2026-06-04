import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import FavoriteCoffees from "./FavoriteCoffees";

vi.mock("../../api/coffeeApi", () => ({
    getFavoriteCoffees: vi.fn(),
    removeCoffeeFavorite: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => vi.fn(),
    };
});

import {
    getFavoriteCoffees,
    removeCoffeeFavorite,
} from "../../api/coffeeApi";

describe("FavoriteCoffees", () => {
    beforeEach(() => {
        vi.mocked(getFavoriteCoffees).mockReset();
        vi.mocked(removeCoffeeFavorite).mockReset();
        vi.spyOn(window, "alert").mockImplementation(() => {});
    });

    it("shows loading state", () => {
        vi.mocked(getFavoriteCoffees).mockImplementation(
            () => new Promise(() => {})
        );

        render(<FavoriteCoffees />);

        expect(screen.getByText("Ładowanie...")).toBeInTheDocument();
    });

    it("shows error message when fetch fails", async () => {
        vi.mocked(getFavoriteCoffees).mockRejectedValue(new Error("fail"));

        render(<FavoriteCoffees />);

        expect(
            await screen.findByText("Nie udało się pobrać ulubionych kaw.")
        ).toBeInTheDocument();
    });

    it("shows empty state when there are no favorites", async () => {
        vi.mocked(getFavoriteCoffees).mockResolvedValue([]);

        render(<FavoriteCoffees />);

        expect(await screen.findByText("Brak ulubionych kaw.")).toBeInTheDocument();
    });

    it("renders favorite coffees from api", async () => {
        vi.mocked(getFavoriteCoffees).mockResolvedValue([
            {
                id: 1,
                name: "Kenia AA",
                region: "Kenia",
                flavorProfiles: ["Owocowe", "Kwiatowe"],
                averageRating: 4.5,
            },
        ]);

        render(<FavoriteCoffees />);

        expect(await screen.findByText("Kenia AA")).toBeInTheDocument();
        expect(screen.getByText("Kenia")).toBeInTheDocument();
        expect(screen.getByText("Owocowe, Kwiatowe")).toBeInTheDocument();
        expect(screen.getByText("4.5")).toBeInTheDocument();
    });

    it("removes coffee from favorites", async () => {
        vi.mocked(getFavoriteCoffees).mockResolvedValue([
            {
                id: 1,
                name: "Kenia AA",
                region: "Kenia",
                averageRating: 4.5,
            },
        ]);
        vi.mocked(removeCoffeeFavorite).mockResolvedValue(null);

        const user = userEvent.setup();

        render(<FavoriteCoffees />);

        await screen.findByText("Kenia AA");

        await user.click(
            screen.getByRole("button", { name: "Usuń z ulubionych" })
        );

        await waitFor(() => {
            expect(removeCoffeeFavorite).toHaveBeenCalledWith(1);
            expect(screen.queryByText("Kenia AA")).not.toBeInTheDocument();
        });
    });

    it("restores list when remove favorite fails", async () => {
        vi.mocked(getFavoriteCoffees).mockResolvedValue([
            {
                id: 1,
                name: "Kenia AA",
                region: "Kenia",
                averageRating: 4.5,
            },
        ]);
        vi.mocked(removeCoffeeFavorite).mockRejectedValue(new Error("fail"));

        const user = userEvent.setup();

        render(<FavoriteCoffees />);

        await screen.findByText("Kenia AA");

        await user.click(
            screen.getByRole("button", { name: "Usuń z ulubionych" })
        );

        await waitFor(() => {
            expect(screen.getByText("Kenia AA")).toBeInTheDocument();
            expect(window.alert).toHaveBeenCalledWith(
                "Nie udało się usunąć z ulubionych."
            );
        });
    });
});
