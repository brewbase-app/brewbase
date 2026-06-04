import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import CuppingPreview from "../CuppingPreview";

const navigateMock = vi.fn();

vi.mock("../../../api/cuppingSessionsApi", () => ({
    getCuppingSessionDetails: vi.fn(),
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
    getCuppingSessionDetails,
} from "../../../api/cuppingSessionsApi";

const sampleSession = {
    id: 1,
    name: "Poranna degustacja",
    sessionDate: "2026-01-15T10:00:00Z",
    createdAt: "2026-01-15T10:00:00Z",
    description: "Test opis sesji",
    coffees: [
        {
            sessionCoffeeId: 10,
            coffeeName: "Kenia AA",
            aromaScore: 8,
            sweetnessScore: 7,
            acidityScore: 6,
            bodyScore: 7,
            overallScore: 8,
            flavorProfileNotes: "Owocowe",
            notes: "Bardzo dobra",
            cleanCup: true,
        },
    ],
};

function renderCuppingPreview() {
    return render(
        <MemoryRouter initialEntries={["/cupping/preview/1"]}>
            <Routes>
                <Route
                    path="/cupping/preview/:id"
                    element={<CuppingPreview />}
                />
            </Routes>
        </MemoryRouter>
    );
}

describe("CuppingPreview", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getCuppingSessionDetails).mockReset();
        vi.mocked(deleteCuppingSession).mockReset();
        vi.spyOn(window, "confirm").mockReturnValue(true);
    });

    it("shows loading state initially", () => {
        vi.mocked(getCuppingSessionDetails).mockImplementation(
            () => new Promise(() => {})
        );

        renderCuppingPreview();

        expect(screen.getByText("Ładowanie sesji...")).toBeInTheDocument();
    });

    it("shows error when session cannot be loaded", async () => {
        vi.mocked(getCuppingSessionDetails).mockRejectedValue(new Error("fail"));

        renderCuppingPreview();

        expect(
            await screen.findByText("Nie udało się pobrać sesji.")
        ).toBeInTheDocument();
    });

    it("renders session preview", async () => {
        vi.mocked(getCuppingSessionDetails).mockResolvedValue(sampleSession);

        renderCuppingPreview();

        expect(
            await screen.findByRole("heading", {
                level: 1,
                name: "Poranna degustacja",
            })
        ).toBeInTheDocument();
        expect(screen.getByText("Kenia AA")).toBeInTheDocument();
        expect(screen.getByText(/Test opis sesji/i)).toBeInTheDocument();
    });

    it("navigates to edit session", async () => {
        vi.mocked(getCuppingSessionDetails).mockResolvedValue(sampleSession);

        const user = userEvent.setup();

        renderCuppingPreview();

        await screen.findByText("Kenia AA");

        await user.click(
            screen.getByRole("button", { name: /Edytuj sesję/i })
        );

        expect(navigateMock).toHaveBeenCalledWith("/cupping/1");
    });

    it("deletes session and returns to list", async () => {
        vi.mocked(getCuppingSessionDetails).mockResolvedValue(sampleSession);
        vi.mocked(deleteCuppingSession).mockResolvedValue(null);

        const user = userEvent.setup();

        renderCuppingPreview();

        await screen.findByText("Kenia AA");

        await user.click(
            screen.getByRole("button", { name: "Usuń sesję" })
        );

        await waitFor(() => {
            expect(deleteCuppingSession).toHaveBeenCalledWith("1");
            expect(navigateMock).toHaveBeenCalledWith("/cupping");
        });
    });
});
