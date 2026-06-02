import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import RegisterPage from "./RegisterPage";

const navigateMock = vi.fn();

vi.mock("../api/apiClient", () => ({
    apiRequest: vi.fn(),
}));

vi.mock("../api/authSession", () => ({
    establishAuthSession: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { apiRequest } from "../api/apiClient";
import { establishAuthSession } from "../api/authSession";

describe("RegisterPage", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(apiRequest).mockReset();
        vi.mocked(establishAuthSession).mockReset();
        vi.mocked(establishAuthSession).mockResolvedValue(undefined);
    });

    it("registers, establishes session and navigates to onboarding", async () => {
        vi.mocked(apiRequest).mockResolvedValue({
            id: 1,
            login: "new-user",
            token: "jwt-token",
        });

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <RegisterPage />
            </MemoryRouter>
        );

        await user.type(
            screen.getByPlaceholderText("Nazwa użytkownika"),
            "new-user"
        );
        await user.type(
            screen.getByPlaceholderText("Adres e-mail"),
            "new-user@brewbase.local"
        );
        await user.type(screen.getByPlaceholderText("Hasło"), "secret123");
        await user.type(
            screen.getByPlaceholderText("Powtórz hasło"),
            "secret123"
        );
        await user.type(
            screen.getByPlaceholderText("Podpowiedź do hasła"),
            "moja podpowiedz"
        );
        await user.click(screen.getByRole("button", { name: "Kontynuuj" }));

        await waitFor(() => {
            expect(establishAuthSession).toHaveBeenCalledWith("jwt-token");
            expect(navigateMock).toHaveBeenCalledWith("/onboarding");
        });
    });

    it("shows error when registration fails", async () => {
        vi.mocked(apiRequest).mockRejectedValue(
            new Error("User with this login already exists")
        );

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <RegisterPage />
            </MemoryRouter>
        );

        await user.type(
            screen.getByPlaceholderText("Nazwa użytkownika"),
            "existing-user"
        );
        await user.type(
            screen.getByPlaceholderText("Adres e-mail"),
            "existing-user@brewbase.local"
        );
        await user.type(screen.getByPlaceholderText("Hasło"), "secret123");
        await user.type(
            screen.getByPlaceholderText("Powtórz hasło"),
            "secret123"
        );
        await user.type(
            screen.getByPlaceholderText("Podpowiedź do hasła"),
            "moja podpowiedz"
        );
        await user.click(screen.getByRole("button", { name: "Kontynuuj" }));

        expect(
            await screen.findByText("User with this login already exists")
        ).toBeInTheDocument();
    });
});
