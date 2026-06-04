import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import Ranking from "../Ranking";

const navigateMock = vi.fn();

vi.mock("../../../api/rankingApi", () => ({
    getCoffeeRanking: vi.fn(),
    getUserRanking: vi.fn(),
    getRecipeRanking: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import {
    getCoffeeRanking,
    getRecipeRanking,
    getUserRanking,
} from "../../../api/rankingApi";

const coffeeRanking = [
    {
        coffeeId: 1,
        position: 1,
        name: "Kenia AA",
        averageRating: 4.8,
        ratingCount: 12,
        region: "Kenia",
        processingMethod: "Washed",
    },
    {
        coffeeId: 2,
        position: 2,
        name: "Etiopia",
        averageRating: 4.5,
        ratingCount: 8,
        region: "Etiopia",
    },
];

const recipeRanking = [
    {
        recipeId: 10,
        position: 1,
        title: "Poranna V60",
        averageRating: 4.9,
        ratingCount: 5,
        saveCount: 3,
        coffee: "Kenia",
        brewingMethod: "V60",
        userLogin: "maria",
    },
];

const userRanking = [
    {
        userId: 1,
        login: "maria",
        position: 1,
        activityScore: 220,
        publicRecipeCount: 4,
        followersCount: 10,
        coffeeRatingCount: 6,
    },
];

describe("Ranking", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getCoffeeRanking).mockReset();
        vi.mocked(getUserRanking).mockReset();
        vi.mocked(getRecipeRanking).mockReset();
    });

    it("renders page header and tabs", async () => {
        vi.mocked(getCoffeeRanking).mockResolvedValue([]);
        vi.mocked(getUserRanking).mockResolvedValue([]);
        vi.mocked(getRecipeRanking).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <Ranking />
            </MemoryRouter>
        );

        expect(screen.getByText("Rankingi")).toBeInTheDocument();
        expect(screen.getByRole("button", { name: /Kawy/i })).toBeInTheDocument();
        expect(screen.getByRole("button", { name: /Receptury/i })).toBeInTheDocument();
        expect(screen.getByRole("button", { name: /Użytkownicy/i })).toBeInTheDocument();

        expect(
            await screen.findByText(
                "Ranking kaw pojawi się po wystawieniu pierwszych ocen."
            )
        ).toBeInTheDocument();
    });

    it("shows coffee ranking error", async () => {
        vi.mocked(getCoffeeRanking).mockRejectedValue(new Error("fail"));
        vi.mocked(getUserRanking).mockResolvedValue([]);
        vi.mocked(getRecipeRanking).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <Ranking />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Nie udało się pobrać rankingu kaw.")
        ).toBeInTheDocument();
    });

    it("renders coffee leaderboard", async () => {
        vi.mocked(getCoffeeRanking).mockResolvedValue(coffeeRanking);
        vi.mocked(getUserRanking).mockResolvedValue([]);
        vi.mocked(getRecipeRanking).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <Ranking />
            </MemoryRouter>
        );

        await screen.findByText("Pełny ranking");

        const leaderboardItems = document.querySelectorAll(".leaderboard-item");
        expect(leaderboardItems).toHaveLength(2);
        expect(leaderboardItems[0]).toHaveTextContent("Kenia AA");
        expect(leaderboardItems[1]).toHaveTextContent("Etiopia");
    });

    it("switches to recipe ranking tab", async () => {
        vi.mocked(getCoffeeRanking).mockResolvedValue([]);
        vi.mocked(getUserRanking).mockResolvedValue([]);
        vi.mocked(getRecipeRanking).mockResolvedValue(recipeRanking);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Ranking />
            </MemoryRouter>
        );

        await user.click(screen.getByRole("button", { name: /Receptury/i }));

        await screen.findByText("Pełny ranking");
        expect(
            document.querySelector(".leaderboard-item")
        ).toHaveTextContent("Poranna V60");
    });

    it("switches to user ranking tab", async () => {
        vi.mocked(getCoffeeRanking).mockResolvedValue([]);
        vi.mocked(getUserRanking).mockResolvedValue(userRanking);
        vi.mocked(getRecipeRanking).mockResolvedValue([]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Ranking />
            </MemoryRouter>
        );

        await user.click(screen.getByRole("button", { name: /Użytkownicy/i }));

        await screen.findByText("Pełny ranking");

        const userItem = document.querySelector(".leaderboard-item");
        expect(userItem).toHaveTextContent("maria");
        expect(userItem).toHaveTextContent("220 pkt");
    });

    it("navigates to coffee details when leaderboard item is clicked", async () => {
        vi.mocked(getCoffeeRanking).mockResolvedValue(coffeeRanking);
        vi.mocked(getUserRanking).mockResolvedValue([]);
        vi.mocked(getRecipeRanking).mockResolvedValue([]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Ranking />
            </MemoryRouter>
        );

        await screen.findByText("Pełny ranking");

        const leaderboardItems = document.querySelectorAll(".leaderboard-item");
        expect(leaderboardItems[0]).toHaveTextContent("Kenia AA");
        await user.click(leaderboardItems[0]);

        expect(navigateMock).toHaveBeenCalledWith("/wiki/coffees/1");
    });
});
