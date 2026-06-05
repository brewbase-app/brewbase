import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import ProfilePage from "../ProfilePage";
import { sampleProfile } from "../../../test/fixtures";

vi.mock("../../../api/profileApi", () => ({
    getProfile: vi.fn(),
}));

vi.mock("../../../api/communityApi", () => ({
    followUser: vi.fn(),
    getFollowers: vi.fn(),
    getFollowing: vi.fn(),
    getUserProfile: vi.fn(),
    getUserProfileByLogin: vi.fn(),
    unfollowUser: vi.fn(),
}));

vi.mock("../../../api/rankingApi", () => ({
    getUserRanking: vi.fn(),
}));

vi.mock("../../../api/recipeApi", () => ({
    getRecipes: vi.fn(),
}));

import { getProfile } from "../../../api/profileApi";
import {
    followUser,
    getFollowers,
    getFollowing,
    getUserProfile,
    getUserProfileByLogin,
} from "../../../api/communityApi";
import { getUserRanking } from "../../../api/rankingApi";
import { getRecipes } from "../../../api/recipeApi";

const publicProfile = {
    userId: 1,
    login: "maria",
    followersCount: 4,
    followingCount: 2,
    recipesCount: 1,
    activityPoints: 150,
};

const otherUserProfile = {
    userId: 2,
    login: "jan",
    followersCount: 10,
    followingCount: 5,
    recipesCount: 3,
    activityPoints: 80,
    isFollowing: false,
};

function renderProfile(initialEntry = "/profile") {
    return render(
        <MemoryRouter initialEntries={[initialEntry]}>
            <Routes>
                <Route path="/profile" element={<ProfilePage />} />
                <Route path="/profile/:username" element={<ProfilePage />} />
            </Routes>
        </MemoryRouter>
    );
}

function mockOwnProfileLoaded() {
    vi.mocked(getProfile).mockResolvedValue(sampleProfile);
    vi.mocked(getUserProfile).mockResolvedValue(publicProfile);
    vi.mocked(getUserRanking).mockResolvedValue([
        { userId: 1, login: "maria", position: 3 },
    ]);
    vi.mocked(getFollowing).mockResolvedValue([]);
    vi.mocked(getFollowers).mockResolvedValue([]);
    vi.mocked(getRecipes).mockResolvedValue([]);
}

describe("ProfilePage", () => {
    beforeEach(() => {
        vi.useRealTimers();
        vi.mocked(getProfile).mockReset();
        vi.mocked(getUserProfile).mockReset();
        vi.mocked(getUserProfileByLogin).mockReset();
        vi.mocked(getUserRanking).mockReset();
        vi.mocked(getFollowing).mockReset();
        vi.mocked(getFollowers).mockReset();
        vi.mocked(getRecipes).mockReset();
    });

    it("shows loading state initially", () => {
        vi.mocked(getProfile).mockImplementation(() => new Promise(() => {}));

        renderProfile();

        expect(screen.getByText("Ładowanie profilu...")).toBeInTheDocument();
    });

    it("shows own profile with edit action", async () => {
        mockOwnProfileLoaded();

        renderProfile();

        expect(
            await screen.findByRole("link", { name: /Edytuj profil/i })
        ).toBeInTheDocument();
        expect(screen.getByText("@maria")).toBeInTheDocument();
        expect(screen.getByText("150")).toBeInTheDocument();
    });

    it("shows error when profile cannot be loaded", async () => {
        vi.mocked(getProfile).mockRejectedValue(new Error("fail"));

        renderProfile();

        expect(
            await screen.findByText("Nie udało się pobrać profilu.")
        ).toBeInTheDocument();
    });

    it(
        "shows Obserwujesz briefly before removing user from discover",
        async () => {
            vi.mocked(getProfile).mockResolvedValue(sampleProfile);
            vi.mocked(getUserProfile).mockResolvedValue(publicProfile);
            vi.mocked(getUserRanking).mockResolvedValue([
                { userId: 1, login: "maria", position: 1 },
                { userId: 3, login: "ania", position: 3 },
            ]);
            vi.mocked(getFollowing).mockResolvedValue([]);
            vi.mocked(getFollowers).mockResolvedValue([]);
            vi.mocked(getRecipes).mockResolvedValue([]);
            vi.mocked(followUser).mockResolvedValue(undefined);

            const user = userEvent.setup();

            renderProfile();

            expect(await screen.findByText("@ania")).toBeInTheDocument();

            await user.click(
                screen.getByRole("button", { name: "Obserwuj" })
            );

            expect(
                await screen.findByRole("button", { name: "Obserwujesz" })
            ).toBeInTheDocument();
            expect(screen.getByText("@ania")).toBeInTheDocument();

            await waitFor(
                () => {
                    expect(screen.queryByText("@ania")).not.toBeInTheDocument();
                },
                { timeout: 3500 }
            );
        },
        5000
    );

    it("hides already followed users from discover section", async () => {
        vi.mocked(getProfile).mockResolvedValue(sampleProfile);
        vi.mocked(getUserProfile).mockResolvedValue(publicProfile);
        vi.mocked(getUserRanking).mockResolvedValue([
            { userId: 1, login: "maria", position: 1 },
            { userId: 2, login: "jan", position: 2 },
            { userId: 3, login: "ania", position: 3 },
        ]);
        vi.mocked(getFollowing).mockResolvedValue([
            { userId: 2, login: "jan", label: null },
        ]);
        vi.mocked(getFollowers).mockResolvedValue([]);
        vi.mocked(getRecipes).mockResolvedValue([]);

        renderProfile();

        expect(await screen.findByText("ODKRYWAJ")).toBeInTheDocument();
        expect(screen.queryByText("@jan")).not.toBeInTheDocument();
        expect(screen.getByText("@ania")).toBeInTheDocument();
    });

    it("shows follow button on another users profile", async () => {
        vi.mocked(getProfile).mockResolvedValue(sampleProfile);
        vi.mocked(getUserProfileByLogin).mockResolvedValue(otherUserProfile);
        vi.mocked(getUserRanking).mockResolvedValue([]);
        vi.mocked(getFollowing).mockResolvedValue([]);
        vi.mocked(getFollowers).mockResolvedValue([]);
        vi.mocked(getRecipes).mockResolvedValue([]);

        renderProfile("/profile/jan");

        expect(await screen.findByText("@jan")).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: "Obserwuj" })
        ).toBeInTheDocument();
        expect(
            screen.queryByRole("link", { name: /Edytuj profil/i })
        ).not.toBeInTheDocument();
    });
});
