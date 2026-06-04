import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";

import Dashboard from "./Dashboard";
import { sampleProfile } from "../../test/fixtures";
import { DASHBOARD_LOAD_ERROR_MESSAGE } from "./loadDashboardData";

vi.mock("./loadDashboardData", () => ({
    loadDashboardData: vi.fn(),
    DASHBOARD_LOAD_ERROR_MESSAGE:
        "Nie udało się załadować danych pulpitu.",
}));

vi.mock("../../components/GlobalSearch", () => ({
    default: () => <div>Global search</div>,
}));

import { loadDashboardData } from "./loadDashboardData";

const emptyDashboardData = {
    profile: null,
    quickNotes: [],
    myRecipes: [],
    cuppingSessions: [],
    cuppingDetails: [],
    recommendedCoffees: [],
    recommendedRecipes: [],
    favoriteCoffees: [],
    favoriteRecipes: [],
    notifications: [],
    followingFeed: [],
};

describe("Dashboard", () => {
    beforeEach(() => {
        vi.mocked(loadDashboardData).mockReset();
    });

    it("shows generic greeting while dashboard data is loading", () => {
        vi.mocked(loadDashboardData).mockImplementation(
            () => new Promise(() => {})
        );

        render(
            <MemoryRouter>
                <Dashboard />
            </MemoryRouter>
        );

        expect(screen.getByText("Dzień dobry!")).toBeInTheDocument();
    });

    it("shows personalized greeting after dashboard data loads", async () => {
        vi.mocked(loadDashboardData).mockResolvedValue({
            ok: true,
            data: {
                ...emptyDashboardData,
                profile: sampleProfile,
            },
        });

        render(
            <MemoryRouter>
                <Dashboard />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Dzień dobry, maria!")
        ).toBeInTheDocument();
    });

    it("shows error message when dashboard data fails to load", async () => {
        vi.mocked(loadDashboardData).mockResolvedValue({
            ok: false,
            error: DASHBOARD_LOAD_ERROR_MESSAGE,
        });

        render(
            <MemoryRouter>
                <Dashboard />
            </MemoryRouter>
        );

        expect(
            await screen.findByText(DASHBOARD_LOAD_ERROR_MESSAGE)
        ).toBeInTheDocument();
    });
});
