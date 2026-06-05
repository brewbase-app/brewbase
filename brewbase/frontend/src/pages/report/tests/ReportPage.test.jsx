import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";

import ReportPage from "../ReportPage";

const navigateMock = vi.fn();

vi.mock("../../../api/reportApi", () => ({
    submitReport: vi.fn(),
}));

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual("react-router-dom");

    return {
        ...actual,
        useNavigate: () => navigateMock,
    };
});

import { submitReport } from "../../../api/reportApi";

const reportTarget = {
    contentType: "coffee",
    contentId: 42,
    contentTitle: "Kenia AA",
    returnPath: "/wiki/coffees/42",
};

function renderReportPage({
    initialEntry = {
        pathname: "/report",
        state: reportTarget,
    },
} = {}) {
    return render(
        <MemoryRouter initialEntries={[initialEntry]}>
            <Routes>
                <Route path="/report" element={<ReportPage />} />
            </Routes>
        </MemoryRouter>
    );
}

describe("ReportPage", () => {
    beforeEach(() => {
        navigateMock.mockReset();
        vi.mocked(submitReport).mockReset();
    });

    it("shows empty state when report target is missing", async () => {
        renderReportPage({
            initialEntry: { pathname: "/report", state: null },
        });

        expect(
            await screen.findByText("Brak treści do zgłoszenia")
        ).toBeInTheDocument();
    });

    it("renders report form for selected content", async () => {
        renderReportPage();

        expect(await screen.findByText("Zgłoś treść")).toBeInTheDocument();
        expect(screen.getByText("Kenia AA")).toBeInTheDocument();
        expect(screen.getByText("Kawa")).toBeInTheDocument();
        expect(screen.getByText("ID: 42")).toBeInTheDocument();
    });

    it("shows validation error when category is not selected", async () => {
        const user = userEvent.setup();

        renderReportPage();

        await screen.findByText("Zgłoś treść");
        await user.click(
            screen.getByRole("button", { name: /Wyślij zgłoszenie/i })
        );

        expect(
            screen.getByText("Wybierz kategorię zgłoszenia.")
        ).toBeInTheDocument();
        expect(submitReport).not.toHaveBeenCalled();
    });

    it("submits report and shows success state", async () => {
        vi.mocked(submitReport).mockResolvedValue({});

        const user = userEvent.setup();

        renderReportPage();

        await screen.findByText("Zgłoś treść");

        await user.selectOptions(
            screen.getByLabelText("Kategoria zgłoszenia"),
            "Spam lub reklama"
        );
        await user.type(
            screen.getByLabelText("Komentarz (opcjonalny)"),
            "Treść wygląda na spam."
        );
        await user.click(
            screen.getByRole("button", { name: /Wyślij zgłoszenie/i })
        );

        await waitFor(() => {
            expect(submitReport).toHaveBeenCalledWith({
                contentType: "coffee",
                contentId: 42,
                contentTitle: "Kenia AA",
                category: "Spam lub reklama",
                comment: "Treść wygląda na spam.",
            });
        });

        expect(await screen.findByText("Zgłoszenie wysłane")).toBeInTheDocument();
        expect(screen.getByText(/Kenia AA/)).toBeInTheDocument();
    });

    it("shows error when submission fails", async () => {
        vi.mocked(submitReport).mockRejectedValue(new Error("fail"));

        const user = userEvent.setup();

        renderReportPage();

        await screen.findByText("Zgłoś treść");

        await user.selectOptions(
            screen.getByLabelText("Kategoria zgłoszenia"),
            "Inny problem"
        );
        await user.click(
            screen.getByRole("button", { name: /Wyślij zgłoszenie/i })
        );

        expect(await screen.findByText("fail")).toBeInTheDocument();
    });

    it("navigates back to content on cancel when returnPath exists", async () => {
        const user = userEvent.setup();

        renderReportPage();

        await screen.findByText("Zgłoś treść");
        await user.click(screen.getByRole("button", { name: "Anuluj" }));

        expect(navigateMock).toHaveBeenCalledWith("/wiki/coffees/42");
    });

    it("navigates back in history when returnPath is missing", async () => {
        const user = userEvent.setup();

        renderReportPage({
            initialEntry: {
                pathname: "/report",
                state: {
                    contentType: "recipe",
                    contentId: 9,
                    contentTitle: "Poranna V60",
                },
            },
        });

        await screen.findByText("Zgłoś treść");
        await user.click(screen.getByRole("button", { name: "Anuluj" }));

        expect(navigateMock).toHaveBeenCalledWith(-1);
    });
});
