import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import Login from "../Login";

const navigateMock = vi.fn();

vi.mock("../../../api/apiClient", async () => {
    const actual = await vi.importActual("../../../api/apiClient");

    return {
        ...actual,
        apiRequest: vi.fn(),
    };
});

vi.mock("../../../api/authSession", () => ({
    establishAuthSession: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { ApiError, apiRequest } from "../../../api/apiClient";
import { establishAuthSession } from "../../../api/authSession";

describe("Login", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(apiRequest).mockReset();
        vi.mocked(establishAuthSession).mockReset();
        vi.mocked(establishAuthSession).mockResolvedValue(undefined);
    });

    it("logs in and navigates to home", async () => {
        vi.mocked(apiRequest).mockResolvedValue({ token: "jwt-token" });

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Login />
            </MemoryRouter>
        );

        await user.type(screen.getByPlaceholderText("login"), "tester");
        await user.type(screen.getByPlaceholderText("hasło"), "secret123");
        await user.click(screen.getByRole("button", { name: "Zaloguj się" }));

        await waitFor(() => {
            expect(establishAuthSession).toHaveBeenCalledWith("jwt-token");
            expect(navigateMock).toHaveBeenCalledWith("/home");
        });
    });

    it("shows password hint after failed login", async () => {
        vi.mocked(apiRequest).mockRejectedValue(
            new ApiError("Unauthorized", 401, {}, "moja podpowiedz")
        );

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Login />
            </MemoryRouter>
        );

        await user.type(screen.getByPlaceholderText("login"), "tester");
        await user.type(screen.getByPlaceholderText("hasło"), "wrong");
        await user.click(screen.getByRole("button", { name: "Zaloguj się" }));

        expect(
            await screen.findByText("Podpowiedź do hasła:")
        ).toBeInTheDocument();
        expect(screen.getByText(/moja podpowiedz/)).toBeInTheDocument();
    });
});
