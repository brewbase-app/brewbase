import { beforeEach, describe, expect, it, vi } from "vitest";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Route, Routes } from "react-router-dom";

import PreferencesOnboardingPage from "../PreferencesOnboardingPage";
import { renderWithRoutes } from "../../../test/testUtils";
import { setAuthToken } from "../../../utils/auth";
import {
    USER_PREFERENCES_STORAGE_KEY,
    DEFAULT_USER_PREFERENCES,
} from "../../../utils/userPreferences";

const navigateMock = vi.fn();

vi.mock("../../../api/preferenceApi", () => ({
    savePreferences: vi.fn().mockResolvedValue(null),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { savePreferences } from "../../../api/preferenceApi";

describe("PreferencesOnboardingPage", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(savePreferences).mockClear();
        vi.mocked(savePreferences).mockResolvedValue(null);
    });

    it("redirects unauthenticated users to login", () => {
        renderWithRoutes(
            <>
                <Route
                    path="/onboarding"
                    element={<PreferencesOnboardingPage />}
                />
                <Route path="/login" element={<p>Login page</p>} />
            </>,
            { initialEntries: ["/onboarding"] }
        );

        expect(screen.getByText("Login page")).toBeInTheDocument();
    });

    it("shows first onboarding step for authenticated users", () => {
        setAuthToken("jwt-token");

        renderWithRoutes(
            <Route
                path="/onboarding"
                element={<PreferencesOnboardingPage />}
            />,
            { initialEntries: ["/onboarding"] }
        );

        expect(screen.getByText("Krok 1 z 6")).toBeInTheDocument();
        expect(
            screen.getByText("Jak oceniasz swoją wiedzę o kawie?")
        ).toBeInTheDocument();
    });

    it("saves preferences and navigates to home on finish", async () => {
        setAuthToken("jwt-token");

        const user = userEvent.setup();

        renderWithRoutes(
            <Route
                path="/onboarding"
                element={<PreferencesOnboardingPage />}
            />,
            { initialEntries: ["/onboarding"] }
        );

        for (let step = 0; step < 5; step += 1) {
            await user.click(screen.getByRole("button", { name: "Dalej" }));
        }

        await user.click(screen.getByRole("button", { name: "Zakończ" }));

        await waitFor(() => {
            expect(savePreferences).toHaveBeenCalledTimes(1);
            expect(navigateMock).toHaveBeenCalledWith("/home");
        });

        /*const stored = JSON.parse(
            localStorage.getItem(USER_PREFERENCES_STORAGE_KEY)
        );

        expect(stored).toEqual(DEFAULT_USER_PREFERENCES);*/
    });
});
