import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import EditProfilePage from "../EditProfilePage";
import { sampleProfile } from "../../../test/fixtures";

vi.mock("../../../api/profileApi", () => ({
    getProfile: vi.fn(),
    updateProfile: vi.fn(),
}));

vi.mock("../../../api/flavorProfileApi", () => ({
    getFlavorProfiles: vi.fn(),
}));

import { getProfile, updateProfile } from "../../../api/profileApi";
import { getFlavorProfiles } from "../../../api/flavorProfileApi";

describe("EditProfilePage", () => {
    beforeEach(() => {
        vi.mocked(getProfile).mockReset();
        vi.mocked(updateProfile).mockReset();
        vi.mocked(getFlavorProfiles).mockReset();
        vi.spyOn(window, "alert").mockImplementation(() => {});
        localStorage.clear();
    });

    it("shows loading state initially", () => {
        vi.mocked(getProfile).mockImplementation(() => new Promise(() => {}));
        vi.mocked(getFlavorProfiles).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <EditProfilePage />
            </MemoryRouter>
        );

        expect(screen.getByText("Ładowanie profilu...")).toBeInTheDocument();
    });

    it("loads profile data into the form", async () => {
        vi.mocked(getProfile).mockResolvedValue({
            ...sampleProfile,
            email: "maria@brewbase.local",
        });
        vi.mocked(getFlavorProfiles).mockResolvedValue([
            { id: 1, name: "Owocowe" },
        ]);

        render(
            <MemoryRouter>
                <EditProfilePage />
            </MemoryRouter>
        );

        expect(await screen.findByDisplayValue("maria")).toBeInTheDocument();
        expect(screen.getByDisplayValue("maria@brewbase.local")).toBeInTheDocument();
        expect(screen.getByRole("button", { name: /Zapisz zmiany/i })).toBeInTheDocument();
    });

    it("shows validation error when passwords do not match", async () => {
        vi.mocked(getProfile).mockResolvedValue({
            ...sampleProfile,
            email: "maria@brewbase.local",
        });
        vi.mocked(getFlavorProfiles).mockResolvedValue([]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <EditProfilePage />
            </MemoryRouter>
        );

        await screen.findByDisplayValue("maria");

        const passwordFields = document.querySelectorAll(
            'input[type="password"]'
        );

        await user.type(passwordFields[1], "secret123");
        await user.type(passwordFields[2], "different");
        await user.click(screen.getByRole("button", { name: /Zapisz zmiany/i }));

        expect(
            await screen.findByText("Nowe hasła nie są identyczne.")
        ).toBeInTheDocument();
        expect(updateProfile).not.toHaveBeenCalled();
    });

    it("saves profile changes", async () => {
        vi.mocked(getProfile).mockResolvedValue({
            ...sampleProfile,
            email: "maria@brewbase.local",
        });
        vi.mocked(getFlavorProfiles).mockResolvedValue([]);
        vi.mocked(updateProfile).mockResolvedValue(null);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <EditProfilePage />
            </MemoryRouter>
        );

        await screen.findByDisplayValue("maria");

        const usernameInput = screen.getByDisplayValue("maria");
        await user.clear(usernameInput);
        await user.type(usernameInput, "maria-updated");
        await user.click(screen.getByRole("button", { name: /Zapisz zmiany/i }));

        await waitFor(() => {
            expect(updateProfile).toHaveBeenCalledWith({
                login: "maria-updated",
                email: "maria@brewbase.local",
                currentPassword: "",
                newPassword: "",
            });
            expect(window.alert).toHaveBeenCalledWith("Zmiany zostały zapisane.");
        });
    });
});
