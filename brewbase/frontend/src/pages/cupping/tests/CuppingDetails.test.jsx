import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import CuppingDetails from "../CuppingDetails";

const navigateMock = vi.fn();

vi.mock("../../../api/cuppingSessionsApi", () => ({
    addCoffeeToCuppingSession: vi.fn(),
    deleteCuppingSession: vi.fn(),
    deleteCuppingSessionCoffee: vi.fn(),
    getCuppingSessionDetails: vi.fn(),
    updateCuppingSession: vi.fn(),
    updateCuppingSessionCoffee: vi.fn(),
}));

vi.mock("../../../api/coffeeApi", () => ({
    getCoffees: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
        useParams: () => ({ id: "1" }),
    };
});

import {
    getCuppingSessionDetails,
    updateCuppingSession,
    updateCuppingSessionCoffee,
} from "../../../api/cuppingSessionsApi";
import { getCoffees } from "../../../api/coffeeApi";

const sampleSession = {
    id: 1,
    name: "Poranna degustacja",
    sessionDate: "2026-01-15T10:00:00Z",
    createdAt: "2026-01-15T10:00:00Z",
    description: "Opis sesji",
    coffees: [
        {
            sessionCoffeeId: 10,
            coffeeId: 5,
            coffeeName: "Kenia AA",
            aromaScore: 8,
            sweetnessScore: 7,
            acidityScore: 6,
            bodyScore: 7,
            overallScore: 8,
            flavorProfileNotes: "Owocowe",
            notes: "Dobra kawa",
            cleanCup: true,
        },
    ],
};

function renderCuppingDetails() {
    return render(
        <MemoryRouter initialEntries={["/cupping/1"]}>
            <Routes>
                <Route path="/cupping/:id" element={<CuppingDetails />} />
            </Routes>
        </MemoryRouter>
    );
}

describe("CuppingDetails", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(getCuppingSessionDetails).mockReset();
        vi.mocked(getCoffees).mockReset();
        vi.mocked(updateCuppingSession).mockReset();
        vi.mocked(updateCuppingSessionCoffee).mockReset();
    });

    it("shows loading state initially", () => {
        vi.mocked(getCuppingSessionDetails).mockImplementation(
            () => new Promise(() => {})
        );
        vi.mocked(getCoffees).mockResolvedValue([]);

        renderCuppingDetails();

        expect(screen.getByText("Ładowanie sesji...")).toBeInTheDocument();
    });

    it("shows error when session cannot be loaded", async () => {
        vi.mocked(getCuppingSessionDetails).mockRejectedValue(new Error("fail"));
        vi.mocked(getCoffees).mockResolvedValue([]);

        renderCuppingDetails();

        expect(
            await screen.findByText("Nie udało się pobrać szczegółów sesji.")
        ).toBeInTheDocument();
    });

    it("loads session data into form", async () => {
        vi.mocked(getCuppingSessionDetails).mockResolvedValue(sampleSession);
        vi.mocked(getCoffees).mockResolvedValue([
            { id: 5, name: "Kenia AA" },
        ]);

        renderCuppingDetails();

        expect(
            await screen.findByDisplayValue("Poranna degustacja")
        ).toBeInTheDocument();
        expect(screen.getByDisplayValue("Opis sesji")).toBeInTheDocument();
        expect(screen.getByText("Degustacja 1")).toBeInTheDocument();
    });

    it("shows validation error when session name is empty", async () => {
        vi.mocked(getCuppingSessionDetails).mockResolvedValue(sampleSession);
        vi.mocked(getCoffees).mockResolvedValue([]);

        const user = userEvent.setup();

        renderCuppingDetails();

        const nameInput = await screen.findByLabelText("Nazwa sesji");
        await user.clear(nameInput);
        await user.click(screen.getByRole("button", { name: "Zapisz sesję" }));

        expect(
            await screen.findByText("Nazwa sesji jest wymagana.")
        ).toBeInTheDocument();
        expect(updateCuppingSession).not.toHaveBeenCalled();
    });

    it("saves session and navigates to preview", async () => {
        vi.mocked(getCuppingSessionDetails).mockResolvedValue(sampleSession);
        vi.mocked(getCoffees).mockResolvedValue([]);
        vi.mocked(updateCuppingSession).mockResolvedValue(null);
        vi.mocked(updateCuppingSessionCoffee).mockResolvedValue(null);

        const user = userEvent.setup();

        renderCuppingDetails();

        await screen.findByDisplayValue("Poranna degustacja");

        await user.click(screen.getByRole("button", { name: "Zapisz sesję" }));

        await waitFor(() => {
            expect(updateCuppingSession).toHaveBeenCalledWith("1", {
                name: "Poranna degustacja",
                description: "Opis sesji",
                sessionDate: "2026-01-15T00:00:00",
            });
            expect(updateCuppingSessionCoffee).toHaveBeenCalled();
            expect(navigateMock).toHaveBeenCalledWith("/cupping/preview/1");
        });
    });
});
