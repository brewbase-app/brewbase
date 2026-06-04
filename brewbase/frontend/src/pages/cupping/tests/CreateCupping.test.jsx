import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import CreateCupping from "../CreateCupping";

const navigateMock = vi.fn();

vi.mock("../../../api/cuppingSessionsApi", () => ({
    createCuppingSession: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { createCuppingSession } from "../../../api/cuppingSessionsApi";

describe("CreateCupping", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(createCuppingSession).mockReset();
    });

    it("renders create session form", () => {
        render(
            <MemoryRouter>
                <CreateCupping />
            </MemoryRouter>
        );

        expect(screen.getByText("Nowa sesja cupping")).toBeInTheDocument();
        expect(screen.getByLabelText("Nazwa sesji")).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: "Rozpocznij sesję" })
        ).toBeDisabled();
    });

    it("creates session and navigates to details", async () => {
        vi.mocked(createCuppingSession).mockResolvedValue({ id: 7 });

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <CreateCupping />
            </MemoryRouter>
        );

        await user.type(
            screen.getByLabelText("Nazwa sesji"),
            "Nowa sesja testowa"
        );
        await user.click(
            screen.getByRole("button", { name: "Rozpocznij sesję" })
        );

        await waitFor(() => {
            expect(createCuppingSession).toHaveBeenCalledWith({
                name: "Nowa sesja testowa",
                description: null,
                sessionDate: null,
            });
            expect(navigateMock).toHaveBeenCalledWith("/cupping/7");
        });
    });

    it("shows error when creation fails", async () => {
        vi.mocked(createCuppingSession).mockRejectedValue(
            new Error("Nie udało się utworzyć sesji.")
        );

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <CreateCupping />
            </MemoryRouter>
        );

        await user.type(screen.getByLabelText("Nazwa sesji"), "Sesja");
        await user.click(
            screen.getByRole("button", { name: "Rozpocznij sesję" })
        );

        expect(
            await screen.findByText("Nie udało się utworzyć sesji.")
        ).toBeInTheDocument();
    });

    it("navigates back to list on cancel", async () => {
        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <CreateCupping />
            </MemoryRouter>
        );

        await user.click(screen.getByRole("button", { name: "Anuluj" }));

        expect(navigateMock).toHaveBeenCalledWith("/cupping");
    });
});
