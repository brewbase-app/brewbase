import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import CuppingList from "../CuppingList";

const navigateMock = vi.fn();

vi.mock("../../../api/cuppingSessionsApi", () => ({
    getCuppingSessions: vi.fn(),
    deleteCuppingSession: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import {
    deleteCuppingSession,
    getCuppingSessions,
} from "../../../api/cuppingSessionsApi";

const sampleSessions = [
    {
        id: 1,
        name: "Poranna degustacja",
        sessionDate: "2026-01-15T10:00:00Z",
        createdAt: "2026-01-15T10:00:00Z",
        coffeeCount: 2,
    },
];

describe("CuppingList", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getCuppingSessions).mockReset();
        vi.mocked(deleteCuppingSession).mockReset();
        vi.spyOn(window, "confirm").mockReturnValue(true);
    });

    it("shows loading state initially", () => {
        vi.mocked(getCuppingSessions).mockImplementation(
            () => new Promise(() => {})
        );

        render(
            <MemoryRouter>
                <CuppingList />
            </MemoryRouter>
        );

        expect(screen.getByText("Ładowanie sesji...")).toBeInTheDocument();
    });

    it("shows error when sessions cannot be loaded", async () => {
        vi.mocked(getCuppingSessions).mockRejectedValue(new Error("fail"));

        render(
            <MemoryRouter>
                <CuppingList />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Nie udało się pobrać sesji cuppingowych.")
        ).toBeInTheDocument();
    });

    it("shows empty state when there are no sessions", async () => {
        vi.mocked(getCuppingSessions).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <CuppingList />
            </MemoryRouter>
        );

        expect(
            await screen.findByText(/Brak zapisanych sesji/i)
        ).toBeInTheDocument();
    });

    it("renders sessions from api", async () => {
        vi.mocked(getCuppingSessions).mockResolvedValue(sampleSessions);

        render(
            <MemoryRouter>
                <CuppingList />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Poranna degustacja")
        ).toBeInTheDocument();
        expect(screen.getByText(/Ilość kaw: 2/i)).toBeInTheDocument();
    });

    it("navigates to create session form", async () => {
        vi.mocked(getCuppingSessions).mockResolvedValue([]);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <CuppingList />
            </MemoryRouter>
        );

        await screen.findByText(/Brak zapisanych sesji/i);

        await user.click(
            screen.getByRole("button", { name: /Dodaj kolejną sesję/i })
        );

        expect(navigateMock).toHaveBeenCalledWith("/cupping/new");
    });

    it("deletes session after confirmation", async () => {
        vi.mocked(getCuppingSessions).mockResolvedValue(sampleSessions);
        vi.mocked(deleteCuppingSession).mockResolvedValue(null);

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <CuppingList />
            </MemoryRouter>
        );

        await screen.findByText("Poranna degustacja");

        await user.click(screen.getByRole("button", { name: "Usuń" }));

        await waitFor(() => {
            expect(deleteCuppingSession).toHaveBeenCalledWith(1);
            expect(
                screen.queryByText("Poranna degustacja")
            ).not.toBeInTheDocument();
        });
    });
});
