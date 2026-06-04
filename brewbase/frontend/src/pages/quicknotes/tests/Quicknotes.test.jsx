import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";

import Quicknotes from "../Quicknotes";

vi.mock("../../../api/quickNotesApi", () => ({
    getQuickNotes: vi.fn(),
    createQuickNote: vi.fn(),
    updateQuickNote: vi.fn(),
    deleteQuickNote: vi.fn(),
}));

import {
    createQuickNote,
    deleteQuickNote,
    getQuickNotes,
    updateQuickNote,
} from "../../../api/quickNotesApi";

const sampleNotes = [
    {
        id: 1,
        content: "Poranna notatka o V60",
        createdAt: "2026-01-10T10:00:00Z",
        updatedAt: "2026-01-10T10:00:00Z",
    },
    {
        id: 2,
        content: "Druga notatka",
        createdAt: "2026-01-11T10:00:00Z",
        updatedAt: "2026-01-11T10:00:00Z",
    },
];

describe("Quicknotes", () => {
    beforeEach(() => {
        vi.mocked(getQuickNotes).mockReset();
        vi.mocked(createQuickNote).mockReset();
        vi.mocked(updateQuickNote).mockReset();
        vi.mocked(deleteQuickNote).mockReset();
    });

    it("shows loading state initially", () => {
        vi.mocked(getQuickNotes).mockImplementation(() => new Promise(() => {}));

        render(
            <MemoryRouter>
                <Quicknotes />
            </MemoryRouter>
        );

        expect(screen.getByText("Ładowanie notatek...")).toBeInTheDocument();
    });

    it("shows error when notes cannot be loaded", async () => {
        vi.mocked(getQuickNotes).mockRejectedValue(new Error("fail"));

        render(
            <MemoryRouter>
                <Quicknotes />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Nie udało się pobrać notatek.")
        ).toBeInTheDocument();
    });

    it("shows empty state when there are no notes", async () => {
        vi.mocked(getQuickNotes).mockResolvedValue([]);

        render(
            <MemoryRouter>
                <Quicknotes />
            </MemoryRouter>
        );

        expect(
            await screen.findByText(
                /Nie masz jeszcze notatek/i
            )
        ).toBeInTheDocument();
    });

    it("renders notes from api", async () => {
        vi.mocked(getQuickNotes).mockResolvedValue(sampleNotes);

        render(
            <MemoryRouter>
                <Quicknotes />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Poranna notatka o V60")
        ).toBeInTheDocument();
        expect(screen.getByText("Druga notatka")).toBeInTheDocument();
    });

    it("creates a new note", async () => {
        vi.mocked(getQuickNotes).mockResolvedValue([]);
        vi.mocked(createQuickNote).mockResolvedValue({
            id: 3,
            content: "Nowa notatka testowa",
            createdAt: "2026-01-12T10:00:00Z",
            updatedAt: "2026-01-12T10:00:00Z",
        });

        const user = userEvent.setup();

        render(
            <MemoryRouter>
                <Quicknotes />
            </MemoryRouter>
        );

        await screen.findByText(/Nie masz jeszcze notatek/i);

        await user.type(
            screen.getByPlaceholderText("Twoja szybka notatka..."),
            "Nowa notatka testowa"
        );
        await user.click(screen.getByRole("button", { name: "Zapisz notatkę" }));

        await waitFor(() => {
            expect(createQuickNote).toHaveBeenCalledWith("Nowa notatka testowa");
            expect(screen.getByText("Nowa notatka testowa")).toBeInTheDocument();
        });
    });

    it("deletes a note after confirmation", async () => {
        vi.mocked(getQuickNotes).mockResolvedValue(sampleNotes);
        vi.mocked(deleteQuickNote).mockResolvedValue(null);

        const user = userEvent.setup();

        const { container } = render(
            <MemoryRouter>
                <Quicknotes />
            </MemoryRouter>
        );

        await screen.findByText("Poranna notatka o V60");

        const noteCard = screen.getByRole("button", {
            name: /Poranna notatka o V60/i,
        });
        const trashIcon = noteCard.querySelector("svg[class*='trash']");

        expect(trashIcon).toBeTruthy();
        await user.click(trashIcon);

        expect(
            await screen.findByRole("heading", { name: "Usunąć notatkę?" })
        ).toBeInTheDocument();

        await user.click(screen.getByRole("button", { name: "Usuń" }));

        await waitFor(() => {
            expect(deleteQuickNote).toHaveBeenCalledWith(1);
            expect(
                screen.queryByText("Poranna notatka o V60")
            ).not.toBeInTheDocument();
        });

        expect(container).toBeTruthy();
    });
});
